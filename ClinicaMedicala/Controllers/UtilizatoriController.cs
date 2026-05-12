using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-02: management complet conturi (creare, editare, dezactivare)
// REQ-04, REQ-05: acces restrictionat doar la administratori
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class UtilizatoriController : Controller
{
    private readonly IUtilizatorService _utilizatorService;

    public UtilizatoriController(IUtilizatorService utilizatorService)
    {
        _utilizatorService = utilizatorService;
    }

    // ── LISTA UTILIZATORI ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(Rol? filtruRol = null)
    {
        var utilizatori = filtruRol.HasValue
            ? await _utilizatorService.GetByRolAsync(filtruRol.Value)
            : await _utilizatorService.GetAllAsync();

        ViewBag.FiltruRol = filtruRol;
        return View(utilizatori);
    }

    // ── DETALII UTILIZATOR ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var utilizator = await _utilizatorService.GetByIdAsync(id);
        if (utilizator == null) return NotFound();

        return View(utilizator);
    }

    // ── CREARE STAFF (medic / asistent / admin) ───────────────────────────────
    [HttpGet]
    public IActionResult Create() => View(new CreareStaffViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreareStaffViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        Utilizator utilizator;

        try
        {
            utilizator = model.Rol switch
            {
                Rol.Medic => CreeazaMedic(model),
                Rol.Asistent => CreeazaAsistent(model),
                Rol.Admin => CreeazaAdmin(model),
                Rol.Pacient => throw new InvalidOperationException(
                    "Pacienții se înregistrează singuri. Folosiți /Auth/Register."),
                _ => throw new InvalidOperationException("Rol invalid.")
            };

            await _utilizatorService.CreeazaAsync(utilizator, model.Parola);

            TempData["Succes"] = $"Contul pentru {utilizator.Prenume} {utilizator.Nume} a fost creat.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // ── DEZACTIVARE CONT (soft delete - REQ-02) ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dezactiveaza(int id)
    {
        var ok = await _utilizatorService.DezactiveazaAsync(id);
        TempData[ok ? "Succes" : "Eroare"] = ok
            ? "Contul a fost dezactivat."
            : "Contul nu a putut fi dezactivat (este deja inactiv sau inexistent).";

        return RedirectToAction(nameof(Index));
    }

    // ── REACTIVARE CONT ──────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactiveaza(int id)
    {
        var ok = await _utilizatorService.ReactiveazaAsync(id);
        TempData[ok ? "Succes" : "Eroare"] = ok
            ? "Contul a fost reactivat."
            : "Contul nu a putut fi reactivat.";

        return RedirectToAction(nameof(Index));
    }

    // ── Factory methods pentru entități specifice ─────────────────────────────
    private static Medic CreeazaMedic(CreareStaffViewModel m)
    {
        if (string.IsNullOrWhiteSpace(m.Specializare))
            throw new InvalidOperationException("Specializarea este obligatorie pentru medic.");
        if (string.IsNullOrWhiteSpace(m.CodParafa))
            throw new InvalidOperationException("Codul de parafă este obligatoriu pentru medic.");
        if (m.GradProfesional == null)
            throw new InvalidOperationException("Gradul profesional este obligatoriu pentru medic.");
        if (m.CostConsultatie == null)
            throw new InvalidOperationException("Costul consultației este obligatoriu pentru medic.");

        return new Medic
        {
            Nume = m.Nume,
            Prenume = m.Prenume,
            Email = m.Email,
            Telefon = m.Telefon,
            Adresa = m.Adresa,
            Specializare = m.Specializare,
            CodParafa = m.CodParafa,
            GradProfesional = m.GradProfesional.Value,
            CostConsultatie = m.CostConsultatie.Value,
            NumarContractCAS = m.NumarContractCAS
        };
    }

    private static Asistent CreeazaAsistent(CreareStaffViewModel m)
    {
        if (string.IsNullOrWhiteSpace(m.Departament))
            throw new InvalidOperationException("Departamentul este obligatoriu pentru asistent.");
        if (m.Tura == null)
            throw new InvalidOperationException("Tura este obligatorie pentru asistent.");

        return new Asistent
        {
            Nume = m.Nume,
            Prenume = m.Prenume,
            Email = m.Email,
            Telefon = m.Telefon,
            Adresa = m.Adresa,
            Departament = m.Departament,
            Tura = m.Tura.Value
        };
    }

    private static Administrator CreeazaAdmin(CreareStaffViewModel m) => new()
    {
        Nume = m.Nume,
        Prenume = m.Prenume,
        Email = m.Email,
        Telefon = m.Telefon,
        Adresa = m.Adresa
    };
}
