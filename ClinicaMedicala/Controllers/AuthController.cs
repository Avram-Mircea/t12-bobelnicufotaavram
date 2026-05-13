using System.Security.Claims;
using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUtilizatorService _utilizatorService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUtilizatorService utilizatorService,
        IPasswordResetService passwordResetService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _utilizatorService = utilizatorService;
        _passwordResetService = passwordResetService;
        _logger = logger;
    }

    // ── LOGIN ─────────────────────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var rezultat = await _authService.LoginAsync(model.Email, model.Parola, ip, userAgent);

        if (!rezultat.Succes || rezultat.Utilizator == null)
        {
            ModelState.AddModelError(string.Empty, rezultat.Eroare ?? "Autentificare eșuată.");
            return View(model);
        }

        // Construim identitatea cu claims (rol, id, nume)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, rezultat.Utilizator.Id.ToString()),
            new(ClaimTypes.Name, $"{rezultat.Utilizator.Prenume} {rezultat.Utilizator.Nume}"),
            new(ClaimTypes.Email, rezultat.Utilizator.Email),
            new(ClaimTypes.Role, rezultat.Utilizator.Rol.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.TineMaMinte,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.TineMaMinte ? 24 * 14 : 8)
            });

        // Redirect către ReturnUrl dacă e local și sigur, altfel Home
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ── LOGOUT ────────────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ── REGISTER PACIENT (auto-înregistrare) ──────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterPacientViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterPacientViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var pacient = new Pacient
            {
                Nume = model.Nume.Trim(),
                Prenume = model.Prenume.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                Telefon = model.Telefon,
                Adresa = model.Adresa,
                CNP = model.CNP,
                DataNastere = model.DataNastere,
                GrupaSanguina = model.GrupaSanguina,
                AsiguratCNAS = false,
                ContactUrgentaNume = model.ContactUrgentaNume,
                ContactUrgentaTelefon = model.ContactUrgentaTelefon
            };

            await _utilizatorService.CreeazaAsync(pacient, model.Parola);

            TempData["Succes"] = "Cont creat cu succes. Vă puteți autentifica.";
            return RedirectToAction(nameof(Login));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Email), ex.Message);
            return View(model);
        }
    }

    // ── FORGOT PASSWORD ───────────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var token = await _passwordResetService.SolicitaResetAsync(model.Email);

        // Mesaj generic indiferent dacă email-ul există sau nu (evită user enumeration)
        TempData["Info"] = "Dacă există un cont cu acest email, vei primi instrucțiuni de resetare.";

        // În producție: aici se trimite email cu link-ul; pentru moment îl logăm
        if (token != null)
        {
            var resetUrl = Url.Action(nameof(ResetPassword), "Auth", new { token }, Request.Scheme);
            _logger.LogInformation("Link reset parolă pentru {Email}: {Url}", model.Email, resetUrl);
        }

        return RedirectToAction(nameof(Login));
    }

    // ── RESET PASSWORD ────────────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login));

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var ok = await _passwordResetService.ReseteazaParolaAsync(model.Token, model.ParolaNoua);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Token invalid sau expirat. Solicitați un nou link de resetare.");
            return View(model);
        }

        TempData["Succes"] = "Parola a fost resetată. Vă puteți autentifica.";
        return RedirectToAction(nameof(Login));
    }

    // ── ACCESS DENIED ─────────────────────────────────────────────────────────
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
