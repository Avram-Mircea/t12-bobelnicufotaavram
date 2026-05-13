using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Pacienti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-33: medicul vede fișa pacientului.
// REQ-34: pacientul vede propria fișă (controlat la nivel de action).
// REQ-35, REQ-36, REQ-37: adăugare consultații, editare informații, istoric modificări.
[Authorize]
public class FisaMedicalaController : Controller
{
    private readonly IFisaMedicalaService _service;

    public FisaMedicalaController(IFisaMedicalaService service)
    {
        _service = service;
    }

    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    public async Task<IActionResult> Details(int pacientId)
    {
        var fisa = await _service.GetByPacientIdAsync(pacientId);

        // Dacă pacientul nu are încă fișă, oferim un model gol — la Save se va crea.
        if (fisa == null)
        {
            fisa = new FisaMedicala
            {
                PacientId = pacientId,
                DataCreare = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }

        return View(fisa);
    }

    [HttpPost]
    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(FisaMedicala model)
    {
        await _service.CreateOrUpdateAsync(model);
        return RedirectToAction(nameof(Details), new { pacientId = model.PacientId });
    }

    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    public async Task<IActionResult> Raport(int pacientId)
    {
        var fisa = await _service.GetByPacientIdAsync(pacientId);
        if (fisa == null) return NotFound();
        return View(fisa);
    }
}
