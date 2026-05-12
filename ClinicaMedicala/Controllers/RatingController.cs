using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers
{
    public class RatingController : Controller
    {
        private readonly IRatingService _service;

        public RatingController(IRatingService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int medicId)
        {
            var ratings = await _service.GetByMedicId(medicId);

            return View(ratings);
        }

        public async Task<IActionResult> List(int medicId)
        {
            var ratings = await _service.GetForMedic(medicId);
            return View(ratings);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Rating model)
        {
            await _service.Add(model);
            return RedirectToAction("List", new { medicId = model.MedicId });
        }
    }
}
