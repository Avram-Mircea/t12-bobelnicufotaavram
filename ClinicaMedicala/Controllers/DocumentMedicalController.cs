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

        public async Task<IActionResult> Edit(int id)
        {
            var doc = await _service.GetById(id);

            if (doc == null)
                return NotFound();

            return View(doc);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DocumentMedical model)
        {
            await _service.Update(model);

            return RedirectToAction("Index",
                new { pacientId = model.PacientId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int pacientId)
        {
            await _service.Delete(id);

            return RedirectToAction("Index",
                new { pacientId });
        }
    }
}
