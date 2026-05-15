using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-16: management reguli per tip de consultație.
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class ReguliConsultatiiController : Controller
{
    private readonly IReguliConsultatieService _service;

    public ReguliConsultatiiController(IReguliConsultatieService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Folosim varianta care creează rânduri default pentru orice TipProgramare
        // lipsă — altfel adminul vedea tabel gol și nu putea bifa nimic.
        var reguli = await _service.GetAllAsigurandExistentaAsync();
        var model = new ReguliConsultatieViewModel
        {
            Reguli = reguli.Select(r => new ReguliConsultatieViewModel.ReguliConsultatieRow
            {
                Id = r.Id,
                TipProgramare = r.TipProgramare,
                NecesitaAsistent = r.NecesitaAsistent,
                Descriere = r.Descriere
            }).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ReguliConsultatieViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var input = model.Reguli.Select(r => (r.Id, r.NecesitaAsistent, r.Descriere));
        await _service.ActualizeazaAsync(input);

        TempData["Succes"] = "Regulile au fost actualizate.";
        return RedirectToAction(nameof(Index));
    }
}
