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

    // După acțiuni (Confirma/Finalizeaza/Cancel/...), medicul se întoarce la
    // pagina lui dedicată „Programările mele”; restul staff-ului rămâne pe lista
    // globală Programare/Index (care e oricum filtrată per rol — vezi Index()).
    private IActionResult RedirectDupaActiune()
    {
        if (User.IsInRole(Rol.Medic.ToString()))
            return RedirectToAction("Programari", "Medic");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Medicul ar trebui să folosească /Medic/Programari, dar dacă ajunge aici
        // (ex. redirect post-acțiune), îl trimitem la pagina lui dedicată.
        if (User.IsInRole(Rol.Medic.ToString()))
        {
            return RedirectToAction("Programari", "Medic");
        }

        // Asistenta vede doar programările legate de departamentul ei — adică
        // acelea unde medicul are aceeași specializare ca departamentul ei.
        if (User.IsInRole(Rol.Asistent.ToString()) && int.TryParse(userIdClaim, out var idAsistent))
        {
            var asistent = await _ctx.Asistenti.FirstOrDefaultAsync(a => a.Id == idAsistent);
            if (asistent != null)
            {
                var departament = asistent.Departament;
                var programariDept = await _ctx.Programari
                    .Include(p => p.Pacient)
                    .Include(p => p.Medic)
                    .Include(p => p.Resursa)
                    .Where(p => p.Medic.Specializare == departament)
                    .OrderByDescending(p => p.DataStart)
                    .ToListAsync();

                ViewBag.DepartamentAsistent = departament;
                return View(programariDept);
            }
        }

        var programari = await _programareService.GetAllWithRelationsAsync();
        return View(programari);
    }

    // REQ-28, REQ-29, REQ-30, REQ-31, REQ-32 — calendar interactiv cu filtre.
    public async Task<IActionResult> Calendar()
    {
        ViewBag.Medici = await _ctx.Medici
            .OrderBy(m => m.Nume).ThenBy(m => m.Prenume)
            .Select(m => new { m.Id, NumeAfisat = $"Dr. {m.Prenume} {m.Nume} — {m.Specializare}" })
            .ToListAsync();

        ViewBag.Resurse = await _ctx.Resurse
            .Where(r => r.Activ)
            .OrderBy(r => r.Denumire)
            .Select(r => new { r.Id, NumeAfisat = $"{r.Denumire} ({r.Tip.ToString().Replace('_', ' ')})" })
            .ToListAsync();

        return View();
    }

    // REQ-28: evenimente pentru calendar (consumat de FullCalendar).
    // FullCalendar trimite automat ?start=...&end=... la fiecare schimbare de vizualizare,
    // iar JS-ul nostru atașează manual ?medicId=...&resursaId=... din dropdown-uri (REQ-29, REQ-30).
    // Răspunsul e re-cerut la fiecare refetch → vizualizarea se actualizează „în timp real” (REQ-32).
    [HttpGet]
    public async Task<IActionResult> Evenimente(
        DateTime? start, DateTime? end, int? medicId, int? resursaId)
    {
        // Fereastra de timp — FullCalendar o trimite mereu; punem fallback-uri rezonabile
        var inceput = start ?? DateTime.UtcNow.AddDays(-30);
        var sfarsit = end ?? DateTime.UtcNow.AddDays(60);

        var query = _ctx.Programari
            .AsNoTracking()
            .Include(p => p.Pacient)
            .Include(p => p.Medic)
            .Include(p => p.Resursa)
            .Where(p => p.DataStart < sfarsit && p.DataEnd > inceput);

        if (medicId.HasValue && medicId.Value > 0)
            query = query.Where(p => p.MedicId == medicId.Value);

        if (resursaId.HasValue && resursaId.Value > 0)
            query = query.Where(p => p.ResursaId == resursaId.Value);

        var programari = await query.OrderBy(p => p.DataStart).ToListAsync();

        var evenimente = programari.Select(p => new
        {
            id = p.Id,
            title = TitluEveniment(p),
            start = p.DataStart.ToString("o"),
            end = p.DataEnd.ToString("o"),
            backgroundColor = CuloareStatus(p.Status),
            borderColor = CuloareStatus(p.Status),
            url = Url.Action(nameof(Edit), new { id = p.Id }),
            extendedProps = new
            {
                pacient = p.Pacient != null ? $"{p.Pacient.Prenume} {p.Pacient.Nume}" : "—",
                medic = p.Medic != null ? $"Dr. {p.Medic.Prenume} {p.Medic.Nume}" : "—",
                resursa = p.Resursa?.Denumire ?? "—",
                tip = p.TipProgramare.ToString().Replace('_', ' '),
                motiv = p.MotivVizita,
                status = p.Status.ToString().Replace('_', ' ')
            }
        });

        return Json(evenimente);
    }

    // REQ-31: intervale indisponibile (mentenanță resursă + resurse inactive) —
    // returnate ca background events ca să fie marcate vizibil în calendar.
    [HttpGet]
    public async Task<IActionResult> PerioadeIndisponibile(
        DateTime? start, DateTime? end, int? resursaId)
    {
        var inceput = (start ?? DateTime.UtcNow.AddDays(-30)).Date;
        var sfarsit = (end ?? DateTime.UtcNow.AddDays(60)).Date.AddDays(1);

        var query = _ctx.PerioadeMentenanta
            .AsNoTracking()
            .Include(p => p.Resursa)
            .Where(p => p.Inceput < sfarsit && p.Sfarsit >= inceput);

        if (resursaId.HasValue && resursaId.Value > 0)
            query = query.Where(p => p.ResursaId == resursaId.Value);

        var perioade = await query.ToListAsync();

        var evenimente = perioade.Select(p => new
        {
            id = $"mentenanta-{p.Id}",
            title = $"🔧 Mentenanță: {p.Resursa.Denumire}",
            start = p.Inceput.ToString("yyyy-MM-dd"),
            end = p.Sfarsit.AddDays(1).ToString("yyyy-MM-dd"),  // FullCalendar: end e exclusiv
            display = "background",
            backgroundColor = "#dc3545",
            extendedProps = new
            {
                tip = "mentenanta",
                resursa = p.Resursa.Denumire,
                descriere = p.Descriere
            }
        });

        return Json(evenimente);
    }

    // Culori per status — aliniate cu badge-urile din lista de programări
    private static string CuloareStatus(StatusProgramare status) => status switch
    {
        StatusProgramare.Programat => "#ffc107",         // galben — așteaptă confirmare
        StatusProgramare.Confirmat => "#198754",         // verde — confirmată
        StatusProgramare.Finalizat => "#0d6efd",         // albastru — finalizată
        StatusProgramare.Anulat_Pacient => "#6c757d",    // gri — anulat de pacient
        StatusProgramare.Anulat_Clinica => "#dc3545",    // roșu — anulat de clinică
        StatusProgramare.Neprezentare => "#212529",      // negru — neprezentare
        _ => "#0d7377"
    };

    private static string TitluEveniment(Programare p)
    {
        var pacient = p.Pacient != null
            ? $"{p.Pacient.Prenume[..1]}. {p.Pacient.Nume}"
            : "Pacient ?";
        var medic = p.Medic != null ? $"Dr. {p.Medic.Nume}" : "Medic ?";
        return $"{pacient} · {medic}";
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulareDictionare();

        var model = new ProgramareCreateViewModel
        {
            DataStart = DateTime.Now,
            DataEnd = DateTime.Now.AddMinutes(30)
        };

        // Pentru medic, pre-completăm automat câmpul MedicId cu Id-ul lui curent,
        // ca să nu mai trebuiască să se aleagă singur din dropdown.
        if (User.IsInRole(Rol.Medic.ToString()))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out var idMedic))
                model.MedicId = idMedic;
        }

        return View(model);
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
        return RedirectDupaActiune();
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
        return RedirectDupaActiune();
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
            return RedirectDupaActiune();
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
            return RedirectDupaActiune();
        }

        programare.Status = StatusProgramare.Confirmat;
        programare.NotificareTrimisa = true;

        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost confirmată.";
        return RedirectDupaActiune();
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
            return RedirectDupaActiune();
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
        return RedirectDupaActiune();
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
            return RedirectDupaActiune();
        }

        programare.Status = StatusProgramare.Neprezentare;
        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost marcată ca neprezentare.";
        return RedirectDupaActiune();
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
        return RedirectDupaActiune();
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
