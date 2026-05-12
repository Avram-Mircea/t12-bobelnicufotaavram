using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// REQ-02: management complet conturi (creare, editare, dezactivare)
// REQ-04, REQ-05: acces restrictionat doar la administratori
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class UtilizatoriController : Controller
{
    private readonly IUtilizatorService _utilizatorService;
    private readonly Repositories.IAutentificareRepository _autRepo;

    public UtilizatoriController(IUtilizatorService utilizatorService,
                                  Repositories.IAutentificareRepository autRepo)
    {
        _utilizatorService = utilizatorService;
        _autRepo = autRepo;
    }

    // ── LISTA UTILIZATORI ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(Rol? filtruRol = null, string? search = null)
    {
        var utilizatori = filtruRol.HasValue
            ? await _utilizatorService.GetByRolAsync(filtruRol.Value)
            : await _utilizatorService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            utilizatori = utilizatori.Where(u =>
                u.Nume.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.Prenume.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var lista = utilizatori.ToList();
        var ultimeleLogari = await _autRepo.GetUltimeleLogariReusiteAsync(lista.Select(u => u.Id));

        ViewBag.FiltruRol = filtruRol;
        ViewBag.Search = search;
        ViewBag.UltimeleLogari = ultimeleLogari;
        return View(lista);
    }

    // ── ISTORIC AUTENTIFICĂRI (REQ-07) ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Istoric(int id)
    {
        var u = await _utilizatorService.GetByIdAsync(id);
        if (u == null) return NotFound();

        var logari = await _autRepo.GetByUtilizatorAsync(id, top: 100);
        ViewBag.Utilizator = u;
        return View(logari);
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

    // ── EDITARE CONT (REQ-02) ─────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var u = await _utilizatorService.GetByIdAsync(id);
        if (u == null) return NotFound();

        var model = new EditeazaUtilizatorViewModel
        {
            Id = u.Id,
            Email = u.Email,
            Rol = u.Rol.ToString(),
            Nume = u.Nume,
            Prenume = u.Prenume,
            Telefon = u.Telefon,
            Adresa = u.Adresa
        };

        switch (u)
        {
            case Medic m:
                model.Specializare = m.Specializare;
                model.CodParafa = m.CodParafa;
                model.GradProfesional = m.GradProfesional;
                model.CostConsultatie = m.CostConsultatie;
                model.NumarContractCAS = m.NumarContractCAS;
                break;
            case Asistent a:
                model.Departament = a.Departament;
                model.Tura = a.Tura;
                break;
            case Pacient p:
                model.CNP = p.CNP;
                model.DataNastere = p.DataNastere;
                model.GrupaSanguina = p.GrupaSanguina;
                model.AsiguratCNAS = p.AsiguratCNAS;
                model.AlergiiCunoscute = p.AlergiiCunoscute;
                model.ContactUrgentaNume = p.ContactUrgentaNume;
                model.ContactUrgentaTelefon = p.ContactUrgentaTelefon;
                break;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditeazaUtilizatorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var u = await _utilizatorService.GetByIdAsync(model.Id);
        if (u == null) return NotFound();

        // Câmpuri comune
        u.Nume = model.Nume.Trim();
        u.Prenume = model.Prenume.Trim();
        u.Telefon = model.Telefon.Trim();
        u.Adresa = model.Adresa.Trim();

        // Câmpuri specifice
        try
        {
            switch (u)
            {
                case Medic m:
                    if (string.IsNullOrWhiteSpace(model.Specializare)
                        || string.IsNullOrWhiteSpace(model.CodParafa)
                        || model.GradProfesional == null
                        || model.CostConsultatie == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Câmpurile specifice medicului sunt obligatorii.");
                        return RepopuleazaEdit(model, u);
                    }
                    m.Specializare = model.Specializare;
                    m.CodParafa = model.CodParafa;
                    m.GradProfesional = model.GradProfesional.Value;
                    m.CostConsultatie = model.CostConsultatie.Value;
                    m.NumarContractCAS = model.NumarContractCAS;
                    break;

                case Asistent a:
                    if (string.IsNullOrWhiteSpace(model.Departament) || model.Tura == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Departamentul și tura sunt obligatorii.");
                        return RepopuleazaEdit(model, u);
                    }
                    a.Departament = model.Departament;
                    a.Tura = model.Tura.Value;
                    break;

                case Pacient p:
                    p.AsiguratCNAS = model.AsiguratCNAS ?? p.AsiguratCNAS;
                    p.AlergiiCunoscute = model.AlergiiCunoscute;
                    p.ContactUrgentaNume = model.ContactUrgentaNume ?? p.ContactUrgentaNume;
                    p.ContactUrgentaTelefon = model.ContactUrgentaTelefon ?? p.ContactUrgentaTelefon;
                    break;
            }

            await _utilizatorService.ActualizeazaAsync(u);
            TempData["Succes"] = $"Contul pentru {u.Prenume} {u.Nume} a fost actualizat.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Cel mai probabil: CodParafa duplicat (unique index)
            ModelState.AddModelError(string.Empty,
                "Conflict: există deja un utilizator cu acest cod de parafă.");
            return RepopuleazaEdit(model, u);
        }
    }

    // Re-populează câmpurile readonly înainte de a re-randa formularul cu erori
    private IActionResult RepopuleazaEdit(EditeazaUtilizatorViewModel model, Utilizator u)
    {
        model.Email = u.Email;
        model.Rol = u.Rol.ToString();
        if (u is Pacient p)
        {
            model.CNP = p.CNP;
            model.DataNastere = p.DataNastere;
            model.GrupaSanguina = p.GrupaSanguina;
        }
        return View(model);
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

    // ── RESET PAROLA (de către admin) ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> ReseteazaParola(int id)
    {
        var u = await _utilizatorService.GetByIdAsync(id);
        if (u == null) return NotFound();

        return View(new ResetParolaAdminViewModel
        {
            Id = u.Id,
            NumeComplet = $"{u.Prenume} {u.Nume}",
            Email = u.Email
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReseteazaParola(ResetParolaAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Re-populare câmpuri afișate (Nume, Email) dacă lipsesc
            var u = await _utilizatorService.GetByIdAsync(model.Id);
            if (u != null)
            {
                model.NumeComplet = $"{u.Prenume} {u.Nume}";
                model.Email = u.Email;
            }
            return View(model);
        }

        var ok = await _utilizatorService.ReseteazaParolaCaAdminAsync(model.Id, model.ParolaNoua);
        TempData[ok ? "Succes" : "Eroare"] = ok
            ? $"Parola pentru {model.NumeComplet} a fost resetată. Comunic-o utilizatorului prin canal sigur."
            : "Resetarea parolei a eșuat.";

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
