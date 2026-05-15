using System.Security.Claims;
using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// Configurare inițială — afișat doar când baza de date este goală.
// Primul cont creat în aplicație devine automat Administrator, iar
// utilizatorul își introduce singur datele (nu mai folosim credențiale
// implicite hardcodate, care nu sunt sigure în mediul de producție).
[AllowAnonymous]
public class SetupController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IUtilizatorService _utilizatorService;

    public SetupController(ApplicationDbContext context, IUtilizatorService utilizatorService)
    {
        _context = context;
        _utilizatorService = utilizatorService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (await _context.Utilizatori.AnyAsync())
            return RedirectToAction("Index", "Home");

        return View(new SetupAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SetupAdminViewModel model)
    {
        // Dacă între timp s-a creat un cont (race condition / refresh), blocăm.
        if (await _context.Utilizatori.AnyAsync())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid) return View(model);

        try
        {
            var admin = new Administrator
            {
                Nume = model.Nume.Trim(),
                Prenume = model.Prenume.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                Telefon = model.Telefon,
                Adresa = model.Adresa,
                Rol = Rol.Admin
            };

            await _utilizatorService.CreeazaAsync(admin, model.Parola);

            // Logăm utilizatorul automat după creare — experiență fluidă la primul start.
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new(ClaimTypes.Name, $"{admin.Prenume} {admin.Nume}"),
                new(ClaimTypes.Email, admin.Email),
                new(ClaimTypes.Role, admin.Rol.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            TempData["Succes"] = "Contul de administrator a fost creat. Bine ai venit!";
            return RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Email), ex.Message);
            return View(model);
        }
    }
}
