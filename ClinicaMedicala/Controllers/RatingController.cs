using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Pacienti;
using ClinicaMedicala.Services.Programari;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-45..50: Rating medic/pacient + media + restricționare + moderare.
[Authorize]
public class RatingController : Controller
{
    private readonly IRatingService _service;
    private readonly IProgramareService _programareService;

    public RatingController(IRatingService service, IProgramareService programareService)
    {
        _service = service;
        _programareService = programareService;
    }

    public async Task<IActionResult> Index(int medicId, int pacientId)
    {
        var ratings = await _service.GetByMedicIdAsync(medicId);

        ViewBag.AverageRating = await _service.GetAverageRatingForMedicAsync(medicId);
        ViewBag.MedicId = medicId;
        ViewBag.PacientId = pacientId;

        // REQ-49: rating doar după consultație finalizată
        ViewBag.CanRate = await _programareService.HasCompletedConsultationAsync(pacientId, medicId);

        return View(ratings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Rating model)
    {
        var allowed = await _programareService.HasCompletedConsultationAsync(model.PacientId, model.MedicId);

        if (!allowed)
        {
            TempData["Eroare"] = "Nu poți acorda rating fără o consultație finalizată.";
            return RedirectToAction(nameof(Index), new
            {
                medicId = model.MedicId,
                pacientId = model.PacientId
            });
        }

        await _service.AddAsync(model);
        TempData["Succes"] = "Rating-ul a fost înregistrat.";
        return RedirectToAction(nameof(Index), new
        {
            medicId = model.MedicId,
            pacientId = model.PacientId
        });
    }
}
