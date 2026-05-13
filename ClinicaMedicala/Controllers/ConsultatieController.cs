using System.Security.Claims;
using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// REQ-35: medicul adaugă consultații în fișa pacientului.
// REQ-36: medicul editează informațiile clinice.
// REQ-37: păstrăm timestamp LastUpdated pe FisaMedicala la fiecare modificare.
[Authorize(Policy = PoliciiAuth.AdminSauMedic)]
public class ConsultatieController : Controller
{
    private readonly ApplicationDbContext _ctx;

    public ConsultatieController(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    private int IdMedicCurent() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── EDITARE consultație ───────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var consultatie = await _ctx.Consultatii
            .Include(c => c.FisaMedicala).ThenInclude(f => f.Pacient)
            .Include(c => c.Medic)
            .Include(c => c.Programare)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultatie == null) return NotFound();

        // Doar medicul care a făcut consultația (sau admin) poate edita
        if (User.IsInRole(Rol.Medic.ToString()) && consultatie.MedicId != IdMedicCurent())
            return Forbid();

        return View(consultatie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string? simptome, string? diagnostic, string? tratament, string? observatii)
    {
        var consultatie = await _ctx.Consultatii
            .Include(c => c.FisaMedicala)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultatie == null) return NotFound();

        if (User.IsInRole(Rol.Medic.ToString()) && consultatie.MedicId != IdMedicCurent())
            return Forbid();

        consultatie.SimptomePrezentate = simptome?.Trim();
        consultatie.DiagnosticICD10 = diagnostic?.Trim();
        consultatie.TratamentRecomandat = tratament?.Trim();
        consultatie.ObservatiiMedic = observatii?.Trim();

        // REQ-37: timestamp pe fișa medicală
        consultatie.FisaMedicala.LastUpdated = DateTime.UtcNow;

        await _ctx.SaveChangesAsync();

        TempData["Succes"] = "Consultația a fost actualizată.";

        // Redirect la fișa pacientului
        return RedirectToAction("Details", "FisaMedicala", new { pacientId = consultatie.FisaMedicala.PacientId });
    }

    // ── ADAUGARE consultație nouă (de la zero, fără programare) ───────────────
    [HttpGet]
    public async Task<IActionResult> Create(int pacientId)
    {
        // Asigură fișa medicală
        var fisa = await _ctx.FiseMedicale.FirstOrDefaultAsync(f => f.PacientId == pacientId);
        if (fisa == null)
        {
            fisa = new FisaMedicala
            {
                PacientId = pacientId,
                DataCreare = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            _ctx.FiseMedicale.Add(fisa);
            await _ctx.SaveChangesAsync();
        }

        ViewBag.PacientId = pacientId;
        ViewBag.FisaMedicalaId = fisa.Id;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int pacientId, int fisaMedicalaId, string? simptome, string? diagnostic, string? tratament, string? observatii)
    {
        var fisa = await _ctx.FiseMedicale.FirstOrDefaultAsync(f => f.Id == fisaMedicalaId);
        if (fisa == null) return NotFound();

        var idMedic = IdMedicCurent();

        var consultatie = new Consultatie
        {
            Data = DateTime.UtcNow,
            FisaMedicalaId = fisa.Id,
            MedicId = idMedic,
            SimptomePrezentate = simptome?.Trim(),
            DiagnosticICD10 = diagnostic?.Trim(),
            TratamentRecomandat = tratament?.Trim(),
            ObservatiiMedic = observatii?.Trim()
        };

        _ctx.Consultatii.Add(consultatie);
        fisa.LastUpdated = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();

        TempData["Succes"] = "Consultația a fost adăugată în fișa pacientului.";
        return RedirectToAction("Details", "FisaMedicala", new { pacientId });
    }
}
