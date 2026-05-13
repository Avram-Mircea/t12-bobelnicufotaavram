using ClinicaMedicala.Data;
using ClinicaMedicala.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// REQ-50: administratorul moderează comentariile la ratinguri.
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class ModerareRatingController : Controller
{
    private readonly ApplicationDbContext _ctx;

    public ModerareRatingController(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IActionResult> Index(bool? doarNemoderate = null)
    {
        var query = _ctx.Ratinguri
            .Include(r => r.Medic)
            .Include(r => r.Pacient)
            .OrderByDescending(r => r.Data)
            .AsQueryable();

        if (doarNemoderate == true)
            query = query.Where(r => !r.Moderat);

        var ratinguri = await query.ToListAsync();

        ViewBag.DoarNemoderate = doarNemoderate;
        ViewBag.NemoderateCount = await _ctx.Ratinguri.CountAsync(r => !r.Moderat);
        return View(ratinguri);
    }

    // Marchează un rating ca moderat (păstrând conținutul)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcheazaModerat(int id)
    {
        var rating = await _ctx.Ratinguri.FindAsync(id);
        if (rating == null) return NotFound();

        rating.Moderat = true;
        await _ctx.SaveChangesAsync();

        TempData["Succes"] = "Rating-ul a fost marcat ca moderat.";
        return RedirectToAction(nameof(Index));
    }

    // Ascunde un rating (păstrat în DB pentru audit, dar nu mai apare public)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ascunde(int id)
    {
        var rating = await _ctx.Ratinguri.FindAsync(id);
        if (rating == null) return NotFound();

        rating.Vizibil = false;
        rating.Moderat = true;
        await _ctx.SaveChangesAsync();

        TempData["Succes"] = "Rating-ul a fost ascuns din afișarea publică.";
        return RedirectToAction(nameof(Index));
    }

    // Restaurează vizibilitatea
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restaureaza(int id)
    {
        var rating = await _ctx.Ratinguri.FindAsync(id);
        if (rating == null) return NotFound();

        rating.Vizibil = true;
        await _ctx.SaveChangesAsync();

        TempData["Succes"] = "Rating-ul a fost restaurat.";
        return RedirectToAction(nameof(Index));
    }
}
