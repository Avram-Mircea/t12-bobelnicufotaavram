using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Programari;
using ClinicaMedicala.Services.Validare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// REQ-20..27: management programări — staff are acces complet.
// Pacientul accesează creare programare separat (vom adăuga la nevoie).
[Authorize(Policy = PoliciiAuth.StaffClinica)]
public class ProgramareController : Controller
{
    private readonly IProgramareService _programareService;
    private readonly IGenericService<Medic> _medicService;
    private readonly IGenericService<Pacient> _pacientService;
    private readonly IGenericService<Asistent> _asistentService;
    private readonly IGenericService<Resursa> _resursaService;
    private readonly IConstraintValidationService _validator;
    private readonly ApplicationDbContext _ctx;

    public ProgramareController(
        IProgramareService programareService,
        IGenericService<Medic> medicService,
        IGenericService<Pacient> pacientService,
        IGenericService<Asistent> asistentService,
        IGenericService<Resursa> resursaService,
        IConstraintValidationService validator,
        ApplicationDbContext ctx)
    {
        _programareService = programareService;
        _medicService = medicService;
        _pacientService = pacientService;
        _asistentService = asistentService;
        _resursaService = resursaService;
        _validator = validator;
        _ctx = ctx;
    }

    public async Task<IActionResult> Index()
    {
        var programari = await _programareService.GetAllWithRelationsAsync();
        return View(programari);
    }

    public IActionResult Calendar() => View();

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulareDictionare();
        return View(new ProgramareCreateViewModel
        {
            DataStart = DateTime.Now,
            DataEnd = DateTime.Now.AddMinutes(30)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProgramareCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulareDictionare();
            return View(model);
        }

        // REQ-22, REQ-23, REQ-27: verificări de suprapunere (logica colegului)
        if (!await _programareService.MedicEsteDisponibilAsync(model.MedicId, model.DataStart, model.DataEnd))
            ModelState.AddModelError("", "Medicul ales nu este disponibil în acest interval.");

        if (model.ResursaId.HasValue)
        {
            var resursaDisponibila = await _programareService.ResursaEsteDisponibilaAsync(
                model.ResursaId.Value, model.DataStart, model.DataEnd);
            if (!resursaDisponibila)
                ModelState.AddModelError("", "Resursa (sala/aparatul) aleasă nu este disponibilă.");
        }

        // REQ-17, REQ-18: validare constrângeri unificate (validatorul meu)
        var resursaIds = model.ResursaId.HasValue ? new List<int> { model.ResursaId.Value } : new List<int>();
        var cerere = new CerereValidareProgramare
        {
            MedicId = model.MedicId,
            TipProgramare = model.TipProgramare,
            AsistentId = model.AsistentId,
            ResursaIds = resursaIds,
            DataStart = model.DataStart,
            DataEnd = model.DataEnd
        };
        var rezultatValidare = await _validator.ValideazaProgramareAsync(cerere);
        if (!rezultatValidare.EValida)
        {
            foreach (var eroare in rezultatValidare.Erori)
                ModelState.AddModelError("", eroare);
        }

        if (model.DataEnd <= model.DataStart)
            ModelState.AddModelError(nameof(model.DataEnd), "Data de sfârșit trebuie să fie după data de început.");

        if (!ModelState.IsValid)
        {
            await PopulareDictionare();
            return View(model);
        }

        var programare = new Programare
        {
            DataStart = model.DataStart,
            DataEnd = model.DataEnd,
            MotivVizita = model.MotivVizita,
            TipProgramare = model.TipProgramare,
            Status = StatusProgramare.Programat,
            PacientId = model.PacientId,
            MedicId = model.MedicId,
            AsistentId = model.AsistentId,
            ResursaId = model.ResursaId
        };

        await _programareService.AddAsync(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost creată cu succes.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null) return NotFound();

        var model = new ProgramareCreateViewModel
        {
            Id = programare.Id,
            DataStart = programare.DataStart,
            DataEnd = programare.DataEnd,
            MotivVizita = programare.MotivVizita,
            TipProgramare = programare.TipProgramare,
            PacientId = programare.PacientId,
            MedicId = programare.MedicId,
            AsistentId = programare.AsistentId,
            ResursaId = programare.ResursaId
        };

        await PopulareDictionare();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProgramareCreateViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulareDictionare();
            return View(model);
        }

        // Verificare suprapuneri (exclude programarea curentă)
        if (!await _programareService.MedicEsteDisponibilAsync(model.MedicId, model.DataStart, model.DataEnd, id))
            ModelState.AddModelError("", "Medicul ales nu este disponibil în acest interval.");

        if (model.ResursaId.HasValue)
        {
            var resursaDisponibila = await _programareService.ResursaEsteDisponibilaAsync(
                model.ResursaId.Value, model.DataStart, model.DataEnd, id);
            if (!resursaDisponibila)
                ModelState.AddModelError("", "Resursa aleasă nu este disponibilă.");
        }

        if (model.DataEnd <= model.DataStart)
            ModelState.AddModelError(nameof(model.DataEnd), "Data de sfârșit trebuie să fie după data de început.");

        if (!ModelState.IsValid)
        {
            await PopulareDictionare();
            return View(model);
        }

        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null) return NotFound();

        programare.DataStart = model.DataStart;
        programare.DataEnd = model.DataEnd;
        programare.MotivVizita = model.MotivVizita;
        programare.TipProgramare = model.TipProgramare;
        programare.PacientId = model.PacientId;
        programare.MedicId = model.MedicId;
        programare.AsistentId = model.AsistentId;
        programare.ResursaId = model.ResursaId;

        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost actualizată.";
        return RedirectToAction(nameof(Index));
    }

    // REQ-21, REQ-26: asistenta confirmă programările făcute de pacienți
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirma(int id)
    {
        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null) return NotFound();

        if (programare.Status != StatusProgramare.Programat)
        {
            TempData["Eroare"] = "Doar programările în starea 'Programat' pot fi confirmate.";
            return RedirectToAction(nameof(Index));
        }

        // La confirmare validăm complet — dacă tipul cere asistent, trebuie atașat unul
        // (din Edit înainte de Confirma). Pacientul nu poate atașa asistent la solicitare.
        var rezultat = await _validator.ValideazaProgramareAsync(new CerereValidareProgramare
        {
            MedicId = programare.MedicId,
            TipProgramare = programare.TipProgramare,
            AsistentId = programare.AsistentId,
            ResursaIds = programare.ResursaId.HasValue ? new List<int> { programare.ResursaId.Value } : new List<int>(),
            DataStart = programare.DataStart,
            DataEnd = programare.DataEnd,
            EsteSolicitareDePacient = false   // confirmare = validare strictă
        });

        if (!rezultat.EValida)
        {
            TempData["Eroare"] = "Nu poți confirma programarea: " + string.Join(" ", rezultat.Erori) +
                                 " Folosește „Modifică” pentru a completa datele lipsă.";
            return RedirectToAction(nameof(Index));
        }

        programare.Status = StatusProgramare.Confirmat;
        programare.NotificareTrimisa = true;

        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost confirmată.";
        return RedirectToAction(nameof(Index));
    }

    // Finalizare consultație (după ce medicul a terminat vizita)
    // REQ-35: la finalizare se creează automat o entitate Consultatie în fișa pacientului
    // (creând și fișa dacă nu există încă).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalizeaza(int id)
    {
        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null) return NotFound();

        if (programare.Status != StatusProgramare.Confirmat
         && programare.Status != StatusProgramare.Programat)
        {
            TempData["Eroare"] = "Doar programările confirmate/programate pot fi finalizate.";
            return RedirectToAction(nameof(Index));
        }

        // 1. Asigură-te că pacientul are o FisaMedicala — dacă nu, creează una.
        var fisa = await _ctx.FiseMedicale.FirstOrDefaultAsync(f => f.PacientId == programare.PacientId);
        if (fisa == null)
        {
            fisa = new FisaMedicala
            {
                PacientId = programare.PacientId,
                DataCreare = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            _ctx.FiseMedicale.Add(fisa);
            await _ctx.SaveChangesAsync();
        }

        // 2. Evită duplicate: dacă există deja o Consultatie pentru această programare, sărim peste
        var consultatieExistenta = await _ctx.Consultatii
            .FirstOrDefaultAsync(c => c.ProgramareId == programare.Id);

        if (consultatieExistenta == null)
        {
            // Creează o consultație minimală — medicul va completa detaliile ulterior
            var consultatie = new Consultatie
            {
                Data = programare.DataStart.Date,
                FisaMedicalaId = fisa.Id,
                MedicId = programare.MedicId,
                ProgramareId = programare.Id,
                SimptomePrezentate = programare.MotivVizita,
                ObservatiiMedic = $"Consultație din programarea {programare.TipProgramare} ({programare.DataStart:dd.MM.yyyy HH:mm})."
            };
            _ctx.Consultatii.Add(consultatie);

            // Actualizează timestamp fișa
            fisa.LastUpdated = DateTime.UtcNow;
            _ctx.FiseMedicale.Update(fisa);
        }

        // 3. Marchează programarea ca finalizată
        programare.Status = StatusProgramare.Finalizat;
        _programareService.Update(programare);

        await _ctx.SaveChangesAsync();
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost marcată ca finalizată. Consultația a fost adăugată în fișa pacientului.";
        return RedirectToAction(nameof(Index));
    }

    // Pacientul nu s-a prezentat
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcheazaNeprezentare(int id)
    {
        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null) return NotFound();

        if (programare.Status != StatusProgramare.Confirmat
         && programare.Status != StatusProgramare.Programat)
        {
            TempData["Eroare"] = "Doar programările confirmate/programate pot fi marcate ca neprezentare.";
            return RedirectToAction(nameof(Index));
        }

        programare.Status = StatusProgramare.Neprezentare;
        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost marcată ca neprezentare.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string motivAnulare)
    {
        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null) return NotFound();

        programare.Status = StatusProgramare.Anulat_Clinica;
        programare.MotivAnulare = motivAnulare;

        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost anulată.";
        return RedirectToAction(nameof(Index));
    }

    // REQ-15: returnează resursele compatibile cu specializarea unui medic, pentru
    // filtrarea dinamică a dropdown-ului de Resursa pe form-ul Create/Edit.
    [HttpGet]
    public async Task<IActionResult> ResurseCompatibile(int medicId)
    {
        var medic = await _ctx.Medici.FirstOrDefaultAsync(m => m.Id == medicId);
        if (medic == null) return Json(Array.Empty<object>());

        var azi = DateTime.UtcNow.Date;

        var resurse = await _ctx.Resurse
            .Include(r => r.Specializari)
            .Include(r => r.PerioadeMentenanta)
            .Where(r => r.Activ
                     && (r.Stare == StareResursa.Functional || r.Stare == StareResursa.Rezervat)
                     && r.DataScadentaRevizie >= azi
                     && !r.PerioadeMentenanta.Any(p => p.Inceput.Date <= azi && p.Sfarsit.Date >= azi))
            .ToListAsync();

        // Universal (fără specializări) SAU compatibilă cu specializarea medicului
        var compatibile = resurse
            .Where(r => !r.Specializari.Any()
                     || r.Specializari.Any(s =>
                          s.Nume.Equals(medic.Specializare, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(r => r.Tip).ThenBy(r => r.Denumire)
            .Select(r => new
            {
                id = r.Id,
                text = $"{r.Denumire} ({r.Tip.ToString().Replace('_', ' ')})"
            });

        return Json(compatibile);
    }

    private async Task PopulareDictionare()
    {
        var pacienti = (await _pacientService.GetAllAsync())
            .Select(p => new { p.Id, NumeAfisat = $"{p.Prenume} {p.Nume} — CNP {p.CNP}" })
            .OrderBy(p => p.NumeAfisat);
        ViewBag.Pacienti = new SelectList(pacienti, "Id", "NumeAfisat");

        var medici = (await _medicService.GetAllAsync())
            .Select(m => new { m.Id, NumeAfisat = $"Dr. {m.Prenume} {m.Nume} — {m.Specializare}" })
            .OrderBy(m => m.NumeAfisat);
        ViewBag.Medici = new SelectList(medici, "Id", "NumeAfisat");

        var asistenti = (await _asistentService.GetAllAsync())
            .Select(a => new { a.Id, NumeAfisat = $"{a.Prenume} {a.Nume} — {a.Departament}" })
            .OrderBy(a => a.NumeAfisat);
        ViewBag.Asistenti = new SelectList(asistenti, "Id", "NumeAfisat");

        var resurse = (await _resursaService.GetAllAsync())
            .Select(r => new { r.Id, NumeAfisat = $"{r.Denumire} ({r.Tip.ToString().Replace('_', ' ')})" })
            .OrderBy(r => r.NumeAfisat);
        ViewBag.Resurse = new SelectList(resurse, "Id", "NumeAfisat");
    }
}
