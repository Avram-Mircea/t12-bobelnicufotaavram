using System.Security.Claims;
using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Pacienti;
using ClinicaMedicala.Services.Programari;
using ClinicaMedicala.Services.Validare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// REQ-20, REQ-25, REQ-34, REQ-45: zona pacientului — propriul flux self-service.
[Authorize(Policy = PoliciiAuth.DoarPacient)]
public class PacientController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly IFisaMedicalaService _fisaService;
    private readonly IDocumentMedicalService _documentService;
    private readonly IProgramareService _programareService;
    private readonly IConstraintValidationService _validator;
    private readonly IRatingService _ratingService;

    public PacientController(
        ApplicationDbContext ctx,
        IFisaMedicalaService fisaService,
        IDocumentMedicalService documentService,
        IProgramareService programareService,
        IConstraintValidationService validator,
        IRatingService ratingService)
    {
        _ctx = ctx;
        _fisaService = fisaService;
        _documentService = documentService;
        _programareService = programareService;
        _validator = validator;
        _ratingService = ratingService;
    }

    // ID-ul pacientului logat — din claim
    private int IdPacientCurent() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── DASHBOARD ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var id = IdPacientCurent();
        var azi = DateTime.UtcNow.Date;

        ViewBag.ProgramariViitoare = await _ctx.Programari
            .Include(p => p.Medic)
            .Where(p => p.PacientId == id
                     && p.DataStart >= azi
                     && p.Status != StatusProgramare.Anulat_Pacient
                     && p.Status != StatusProgramare.Anulat_Clinica)
            .OrderBy(p => p.DataStart)
            .Take(3)
            .ToListAsync();

        ViewBag.AreFisa = await _fisaService.GetByPacientIdAsync(id) != null;

        return View();
    }

    // ── REQ-34: vizualizare propria fișă medicală ─────────────────────────────
    public async Task<IActionResult> FisaMea()
    {
        var id = IdPacientCurent();
        var fisa = await _fisaService.GetByPacientIdAsync(id);
        ViewBag.Documente = await _documentService.GetByPacientIdAsync(id);
        return View(fisa);
    }

    // ── Lista propriilor programări ───────────────────────────────────────────
    public async Task<IActionResult> ProgramarileMele()
    {
        var id = IdPacientCurent();
        var programari = await _ctx.Programari
            .Include(p => p.Medic)
            .Include(p => p.Resursa)
            .Where(p => p.PacientId == id)
            .OrderByDescending(p => p.DataStart)
            .ToListAsync();
        return View(programari);
    }

    // ── REQ-20: programare online (creare) ────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> NouaProgramare()
    {
        await PopuleazaMedici();
        return View(new ProgramareCreateViewModel
        {
            DataStart = DateTime.Now.Date.AddDays(1).AddHours(9),
            DataEnd = DateTime.Now.Date.AddDays(1).AddHours(10),
            TipProgramare = TipProgramare.Consult_Initial
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NouaProgramare(ProgramareCreateViewModel model)
    {
        // Pacient nu poate alege Pacient (e el însuși), AsistentId, ResursaId
        model.PacientId = IdPacientCurent();
        model.AsistentId = null;
        model.ResursaId = null;
        ModelState.Remove(nameof(model.PacientId));

        if (!ModelState.IsValid)
        {
            await PopuleazaMedici();
            return View(model);
        }

        if (model.DataEnd <= model.DataStart)
            ModelState.AddModelError(nameof(model.DataEnd), "Data de sfârșit trebuie să fie după data de început.");

        if (model.DataStart < DateTime.Now)
            ModelState.AddModelError(nameof(model.DataStart), "Nu poți face programări în trecut.");

        // Disponibilitate medic + validator de constrângeri
        var medicDisponibil = await _programareService.MedicEsteDisponibilAsync(
            model.MedicId, model.DataStart, model.DataEnd);
        if (!medicDisponibil)
            ModelState.AddModelError("", "Medicul ales nu este disponibil în acest interval.");

        var rezultat = await _validator.ValideazaProgramareAsync(new CerereValidareProgramare
        {
            MedicId = model.MedicId,
            TipProgramare = model.TipProgramare,
            AsistentId = null,
            ResursaIds = new List<int>(),
            DataStart = model.DataStart,
            DataEnd = model.DataEnd
        });
        if (!rezultat.EValida)
            foreach (var er in rezultat.Erori)
                ModelState.AddModelError("", er);

        if (!ModelState.IsValid)
        {
            await PopuleazaMedici();
            return View(model);
        }

        var programare = new Programare
        {
            DataStart = model.DataStart,
            DataEnd = model.DataEnd,
            MotivVizita = model.MotivVizita,
            TipProgramare = model.TipProgramare,
            Status = StatusProgramare.Programat,    // așteaptă confirmare asistent
            PacientId = IdPacientCurent(),
            MedicId = model.MedicId
        };

        await _programareService.AddAsync(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea ta a fost creată. Va fi confirmată de o asistentă.";
        return RedirectToAction(nameof(ProgramarileMele));
    }

    // ── REQ-25: anulare propria programare ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnuleazaProgramare(int id, string? motivAnulare)
    {
        var idPacient = IdPacientCurent();
        var programare = await _programareService.GetByIdAsync(id);

        if (programare == null || programare.PacientId != idPacient)
            return NotFound();

        if (programare.Status == StatusProgramare.Anulat_Clinica ||
            programare.Status == StatusProgramare.Anulat_Pacient ||
            programare.Status == StatusProgramare.Finalizat)
        {
            TempData["Eroare"] = "Programarea nu mai poate fi anulată.";
            return RedirectToAction(nameof(ProgramarileMele));
        }

        programare.Status = StatusProgramare.Anulat_Pacient;
        programare.MotivAnulare = string.IsNullOrWhiteSpace(motivAnulare)
            ? "Anulat de pacient"
            : motivAnulare;

        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        TempData["Succes"] = "Programarea a fost anulată.";
        return RedirectToAction(nameof(ProgramarileMele));
    }

    // ── REQ-45: pacientul acordă rating medicului ─────────────────────────────
    [HttpGet]
    public async Task<IActionResult> RatingMedici()
    {
        var idPacient = IdPacientCurent();

        // Medicii cu care pacientul a avut consultații finalizate
        var mediciCuConsultatii = await _ctx.Programari
            .Include(p => p.Medic)
            .Where(p => p.PacientId == idPacient && p.Status == StatusProgramare.Finalizat)
            .Select(p => p.Medic)
            .Distinct()
            .ToListAsync();

        // Pentru fiecare medic, verifică dacă pacientul i-a dat deja rating (direcția pacient→medic)
        var ratinguriDate = await _ctx.Ratinguri
            .Where(r => r.PacientId == idPacient && !r.AcordatDeMedic)
            .Select(r => r.MedicId)
            .ToListAsync();

        ViewBag.RatinguriDate = ratinguriDate;
        return View(mediciCuConsultatii);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcordaRating(int medicId, int scor, string? comentariu)
    {
        var idPacient = IdPacientCurent();

        // REQ-49: doar după consultație finalizată
        var areConsultatie = await _programareService.HasCompletedConsultationAsync(idPacient, medicId);
        if (!areConsultatie)
        {
            TempData["Eroare"] = "Nu poți acorda rating fără o consultație finalizată.";
            return RedirectToAction(nameof(RatingMedici));
        }

        // Nu poți acorda rating dublu aceluiași medic — verificăm doar pe direcția
        // pacient→medic (medicul tot mai poate avea rating-ul lui către tine separat).
        var existaDeja = await _ctx.Ratinguri.AnyAsync(r =>
            r.PacientId == idPacient && r.MedicId == medicId && !r.AcordatDeMedic);
        if (existaDeja)
        {
            TempData["Eroare"] = "Ai acordat deja un rating acestui medic.";
            return RedirectToAction(nameof(RatingMedici));
        }

        if (scor < 1 || scor > 5)
        {
            TempData["Eroare"] = "Scorul trebuie să fie între 1 și 5.";
            return RedirectToAction(nameof(RatingMedici));
        }

        await _ratingService.AddAsync(new Rating
        {
            PacientId = idPacient,
            MedicId = medicId,
            Scor = scor,
            Comentariu = comentariu?.Trim(),
            Data = DateTime.UtcNow,
            Vizibil = true,
            Moderat = false,
            AcordatDeMedic = false   // pacient → medic
        });

        TempData["Succes"] = "Mulțumim pentru rating!";
        return RedirectToAction(nameof(RatingMedici));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task PopuleazaMedici()
    {
        var medici = await _ctx.Medici
            .Where(m => m.StatusCont)
            .OrderBy(m => m.Nume)
            .Select(m => new
            {
                m.Id,
                NumeAfisat = m.Nume + " " + m.Prenume + " — " + m.Specializare
            })
            .ToListAsync();

        ViewBag.Medici = new SelectList(medici, "Id", "NumeAfisat");
    }
}
