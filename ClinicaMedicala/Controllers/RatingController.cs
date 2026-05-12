using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers
{
    public class RatingController : Controller
    {
        private readonly IRatingService _service;
        private readonly IProgramareService _programareService;

        public RatingController(IRatingService service, IProgramareService programareService)
        {
            _service = service;
            _programareService = programareService;
        }

        // LISTARE + FORM (dacă are voie să dea rating)
        public async Task<IActionResult> Index(int medicId, int pacientId)
        {
            var ratings = await _service.GetByMedicId(medicId);

            ViewBag.AverageRating = await _service.GetAverageRatingForMedic(medicId);

            ViewBag.MedicId = medicId;
            ViewBag.PacientId = pacientId;

            ViewBag.CanRate = await _programareService
                .HasCompletedConsultation(pacientId, medicId);

            return View(ratings);
        }

        // ADAUGARE RATING
        [HttpPost]
        public async Task<IActionResult> Add(Rating model)
        {
            var allowed = await _programareService
                .HasCompletedConsultation(model.PacientId, model.MedicId);

            if (!allowed)
            {
                ModelState.AddModelError("",
                    "Nu poți acorda rating fără o consultație finalizată.");

                return RedirectToAction("Index", new
                {
                    medicId = model.MedicId,
                    pacientId = model.PacientId
                });
            }

            await _service.Add(model);

            return RedirectToAction("Index", new
            {
                medicId = model.MedicId,
                pacientId = model.PacientId
            });
        }
    }
}