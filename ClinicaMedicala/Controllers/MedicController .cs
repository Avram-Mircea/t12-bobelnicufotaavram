using ClinicaMedicala.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers
{
    public class MedicController : Controller
    {
        private readonly IProgramareService _programareService;

        public IActionResult Index()
        {
            return View();
        }

        public MedicController(IProgramareService programareService)
        {
            _programareService = programareService;
        }

        public async Task<IActionResult> Programari(int medicId)
        {
            var programari = await _programareService
                .GetProgramariMedic(medicId);

            return View(programari);
        }
    }
}
