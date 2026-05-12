using System.Security.Claims;
using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-08...REQ-14: Management Resurse (cabinete, aparate, săli).
// REQ-04, REQ-05: doar adminul gestionează resursele.
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class ResurseController : Controller
{
    private readonly IResursaService _resurse;

    public ResurseController(IResursaService resurse)
    {
        _resurse = resurse;
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

    // ── CREARE (REQ-09) ───────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Create() => View(new CreareResursaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreareResursaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.DataScadentaRevizie < model.DataUltimaRevizie)
        {
            ModelState.AddModelError(nameof(model.DataScadentaRevizie),
                "Scadența reviziei trebuie să fie după ultima revizie.");
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
                SpecializarePermisa = model.SpecializarePermisa,
                DataUltimaRevizie = model.DataUltimaRevizie,
                DataScadentaRevizie = model.DataScadentaRevizie,
                AdministratorId = IdAdminCurent()
            };

            await _resurse.CreeazaAsync(resursa);

            TempData["Succes"] = $"Resursa „{resursa.Denumire}” a fost adăugată.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // ID-ul adminului din claim — folosit ca FK pentru AdministratorId
    private int IdAdminCurent() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
