using System.Security.Claims;
using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// REQ-08...REQ-14: Management Resurse (cabinete, aparate, săli).
// REQ-04, REQ-05: doar adminul gestionează resursele.
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class ResurseController : Controller
{
    private readonly IResursaService _resurse;
    private readonly ApplicationDbContext _ctx;

    public ResurseController(IResursaService resurse, ApplicationDbContext ctx)
    {
        _resurse = resurse;
        _ctx = ctx;
    }

    // ── LISTĂ + FILTRE ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(TipResursa? tip = null, StareResursa? stare = null, string? search = null)
    {
        var lista = await _resurse.CautaAsync(tip, stare, search);

        ViewBag.FiltruTip = tip;
        ViewBag.FiltruStare = stare;
        ViewBag.Search = search;
        return View(lista);
    }

    // ── CREARE (REQ-09 + REQ-13) ──────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreareResursaViewModel
        {
            SpecializariDisponibile = await SpecializariActiveAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreareResursaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.SpecializariDisponibile = await SpecializariActiveAsync();
            return View(model);
        }

        if (model.DataScadentaRevizie < model.DataUltimaRevizie)
        {
            ModelState.AddModelError(nameof(model.DataScadentaRevizie),
                "Scadența reviziei trebuie să fie după ultima revizie.");
            model.SpecializariDisponibile = await SpecializariActiveAsync();
            return View(model);
        }

        try
        {
            var resursa = new Resursa
            {
                Denumire = model.Denumire,
                Tip = model.Tip,
                NumarInventar = model.NumarInventar,
                Locatie = model.Locatie,
                DataUltimaRevizie = model.DataUltimaRevizie,
                DataScadentaRevizie = model.DataScadentaRevizie,
                AdministratorId = IdAdminCurent()
            };

            await _resurse.CreeazaAsync(resursa, model.SpecializareIds);

            TempData["Succes"] = $"Resursa „{resursa.Denumire}” a fost adăugată.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.SpecializariDisponibile = await SpecializariActiveAsync();
            return View(model);
        }
    }

    // ── EDITARE (REQ-10) ──────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var resursa = await _ctx.Resurse
            .Include(r => r.Specializari)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resursa == null) return NotFound();

        var model = new EditeazaResursaViewModel
        {
            Id = resursa.Id,
            Denumire = resursa.Denumire,
            Tip = resursa.Tip,
            NumarInventar = resursa.NumarInventar,
            Locatie = resursa.Locatie,
            Stare = resursa.Stare,
            DataUltimaRevizie = resursa.DataUltimaRevizie,
            DataScadentaRevizie = resursa.DataScadentaRevizie,
            SpecializareIds = resursa.Specializari.Select(s => s.Id).ToList(),
            SpecializariDisponibile = await SpecializariActiveAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditeazaResursaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.SpecializariDisponibile = await SpecializariActiveAsync();
            return View(model);
        }

        if (model.DataScadentaRevizie < model.DataUltimaRevizie)
        {
            ModelState.AddModelError(nameof(model.DataScadentaRevizie),
                "Scadența reviziei trebuie să fie după ultima revizie.");
            model.SpecializariDisponibile = await SpecializariActiveAsync();
            return View(model);
        }

        try
        {
            await _resurse.ActualizeazaAsync(
                model.Id,
                model.Denumire,
                model.Tip,
                model.NumarInventar,
                model.Locatie,
                model.Stare,
                model.DataUltimaRevizie,
                model.DataScadentaRevizie,
                model.SpecializareIds);

            TempData["Succes"] = $"Resursa „{model.Denumire}” a fost actualizată.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.SpecializariDisponibile = await SpecializariActiveAsync();
            return View(model);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private Task<List<Specializare>> SpecializariActiveAsync() =>
        _ctx.Specializari.Where(s => s.Activ).OrderBy(s => s.Nume).ToListAsync();

    private int IdAdminCurent() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
