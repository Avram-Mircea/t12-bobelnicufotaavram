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
    public async Task<IActionResult> Index(TipResursa? tip = null, StareResursa? stare = null, string? search = null, bool? doarActive = null)
    {
        var lista = await _resurse.CautaAsync(tip, stare, search, doarActive);

        ViewBag.FiltruTip = tip;
        ViewBag.FiltruStare = stare;
        ViewBag.Search = search;
        ViewBag.DoarActive = doarActive;
        return View(lista);
    }

    // ── ACTIVARE/DEZACTIVARE (REQ-11, REQ-12) ─────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dezactiveaza(int id)
    {
        var ok = await _resurse.DezactiveazaAsync(id);
        TempData[ok ? "Succes" : "Eroare"] = ok
            ? "Resursa a fost dezactivată. Nu va mai apărea în calendar pentru programări noi."
            : "Resursa nu a putut fi dezactivată (poate e deja inactivă).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activeaza(int id)
    {
        var ok = await _resurse.ActiveazaAsync(id);
        TempData[ok ? "Succes" : "Eroare"] = ok
            ? "Resursa a fost reactivată și este disponibilă pentru programări noi."
            : "Resursa nu a putut fi activată.";
        return RedirectToAction(nameof(Index));
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

        try
        {
            // Datele de revizie: defaults aplicate de service (azi + 1 an).
            // Adminul gestionează mentenanța ulterior din pagina dedicată.
            var resursa = new Resursa
            {
                Denumire = model.Denumire,
                Tip = model.Tip,
                NumarInventar = model.NumarInventar,
                Locatie = model.Locatie,
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

    // ── PERIOADE MENTENANTA (REQ-14) ──────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Mentenanta(int id)
    {
        var resursa = await _resurse.GetByIdAsync(id);
        if (resursa == null) return NotFound();

        ViewBag.Resursa = resursa;
        var perioade = await _resurse.GetPerioadeAsync(id);
        return View(perioade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdaugaPerioada(int resursaId, DateTime inceput, DateTime sfarsit, string? descriere)
    {
        try
        {
            await _resurse.AdaugaPerioadaAsync(resursaId, inceput, sfarsit, descriere);
            TempData["Succes"] = $"Perioadă de mentenanță adăugată ({inceput:dd.MM.yyyy} - {sfarsit:dd.MM.yyyy}).";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Eroare"] = ex.Message;
        }
        return RedirectToAction(nameof(Mentenanta), new { id = resursaId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StergePerioada(int perioadaId, int resursaId)
    {
        var ok = await _resurse.StergePerioadaAsync(perioadaId);
        TempData[ok ? "Succes" : "Eroare"] = ok
            ? "Perioada de mentenanță a fost ștearsă."
            : "Perioada nu a putut fi ștearsă.";
        return RedirectToAction(nameof(Mentenanta), new { id = resursaId });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private Task<List<Specializare>> SpecializariActiveAsync() =>
        _ctx.Specializari.Where(s => s.Activ).OrderBy(s => s.Nume).ToListAsync();

    private int IdAdminCurent() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
