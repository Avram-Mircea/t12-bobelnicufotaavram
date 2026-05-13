using System.Security.Claims;
using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Pacienti;
using ClinicaMedicala.Services.Programari;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// REQ-33, REQ-35, REQ-36, REQ-46: zona medicului — pacienții și consultațiile sale.
[Authorize(Policy = PoliciiAuth.DoarMedic)]
public class MedicController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly IProgramareService _programareService;
    private readonly IRatingService _ratingService;

    public MedicController(
        ApplicationDbContext ctx,
        IProgramareService programareService,
        IRatingService ratingService)
    {
        _ctx = ctx;
        _programareService = programareService;
        _ratingService = ratingService;
    }

    private int IdMedicCurent() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── DASHBOARD ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var idMedic = IdMedicCurent();
        var azi = DateTime.UtcNow.Date;
        var maine = azi.AddDays(1);

        ViewBag.ProgramariAzi = await _ctx.Programari
            .Include(p => p.Pacient)
            .Where(p => p.MedicId == idMedic
                     && p.DataStart >= azi && p.DataStart < maine
                     && p.Status != StatusProgramare.Anulat_Pacient
                     && p.Status != StatusProgramare.Anulat_Clinica)
            .OrderBy(p => p.DataStart)
            .ToListAsync();

        ViewBag.NumarPacientiUnici = await _ctx.Programari
            .Where(p => p.MedicId == idMedic)
            .Select(p => p.PacientId)
            .Distinct()
            .CountAsync();

        ViewBag.RatingMediu = await _ratingService.GetAverageRatingForMedicAsync(idMedic);

        return View();
    }

    // ── REQ-33: lista propriilor pacienți ─────────────────────────────────────
    public async Task<IActionResult> Pacientii()
    {
        var idMedic = IdMedicCurent();

        var pacientiIds = await _ctx.Programari
            .Where(p => p.MedicId == idMedic)
            .Select(p => p.PacientId)
            .Distinct()
            .ToListAsync();

        var pacienti = await _ctx.Pacienti
            .Where(p => pacientiIds.Contains(p.Id))
            .OrderBy(p => p.Nume)
            .ToListAsync();

        return View(pacienti);
    }

    // ── REQ-33: detalii pacient + acțiuni rapide ──────────────────────────────
    public async Task<IActionResult> DetaliuPacient(int id)
    {
        var idMedic = IdMedicCurent();

        // Verifică: pacientul trebuie să aibă cel puțin o programare cu acest medic
        var areAcces = await _ctx.Programari.AnyAsync(p => p.MedicId == idMedic && p.PacientId == id);
        if (!areAcces) return Forbid();

        var pacient = await _ctx.Pacienti
            .Include("FisaMedicala")
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pacient == null) return NotFound();

        ViewBag.Programari = await _ctx.Programari
            .Where(p => p.MedicId == idMedic && p.PacientId == id)
            .OrderByDescending(p => p.DataStart)
            .ToListAsync();

        ViewBag.AcordaRatingDisponibil = await _programareService
            .HasCompletedConsultationAsync(id, idMedic);

        return View(pacient);
    }

    // ── REQ-46: medicul acordă rating pacientului ────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcordaRatingPacient(int pacientId, int scor, string? comentariu)
    {
        var idMedic = IdMedicCurent();

        var areConsultatie = await _programareService.HasCompletedConsultationAsync(pacientId, idMedic);
        if (!areConsultatie)
        {
            TempData["Eroare"] = "Nu poți acorda rating fără o consultație finalizată.";
            return RedirectToAction(nameof(DetaliuPacient), new { id = pacientId });
        }

        if (scor < 1 || scor > 5)
        {
            TempData["Eroare"] = "Scorul trebuie să fie între 1 și 5.";
            return RedirectToAction(nameof(DetaliuPacient), new { id = pacientId });
        }

        // Verifică unicitate doar pe direcția medic→pacient (pacientul tot poate avea
        // rating-ul lui spre medic separat).
        var exista = await _ctx.Ratinguri.AnyAsync(r =>
            r.MedicId == idMedic && r.PacientId == pacientId && r.AcordatDeMedic);
        if (exista)
        {
            TempData["Eroare"] = "Ai acordat deja un rating acestui pacient.";
            return RedirectToAction(nameof(DetaliuPacient), new { id = pacientId });
        }

        await _ratingService.AddAsync(new Rating
        {
            PacientId = pacientId,
            MedicId = idMedic,
            Scor = scor,
            Comentariu = comentariu?.Trim(),
            Data = DateTime.UtcNow,
            Vizibil = true,
            Moderat = false,
            AcordatDeMedic = true   // medic → pacient
        });

        TempData["Succes"] = "Rating-ul a fost înregistrat.";
        return RedirectToAction(nameof(DetaliuPacient), new { id = pacientId });
    }

    // ── REQ-33: programările proprii ──────────────────────────────────────────
    public async Task<IActionResult> Programari()
    {
        var idMedic = IdMedicCurent();
        var programari = await _programareService.GetProgramariMedicAsync(idMedic);
        return View(programari);
    }
}
