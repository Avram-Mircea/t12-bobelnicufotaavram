using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers
{
    public class DocumentMedicalController : Controller
    {
        private readonly IDocumentMedicalService _service;

        public DocumentMedicalController(IDocumentMedicalService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int pacientId)
        {
            var docs = await _service.GetByPacientId(pacientId);
            return View(docs);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(DocumentMedical model)
        {
            await _service.Add(model);
            return RedirectToAction("Index", new { pacientId = model.PacientId });
        }
    }
}
