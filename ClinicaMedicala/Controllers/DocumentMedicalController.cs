using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Pacienti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-38: încărcare documente externe.
// REQ-39: editare informații asociate.
// REQ-40: ștergere documente.
// REQ-41: asociere cu pacient.
[Authorize(Policy = PoliciiAuth.AdminSauMedic)]
public class DocumentMedicalController : Controller
{
    private readonly IDocumentMedicalService _service;

    public DocumentMedicalController(IDocumentMedicalService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int pacientId)
    {
        var docs = await _service.GetByPacientIdAsync(pacientId);
        ViewBag.PacientId = pacientId;
        return View(docs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentMedical model)
    {
        await _service.AddAsync(model);
        return RedirectToAction(nameof(Index), new { pacientId = model.PacientId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _service.GetByIdAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DocumentMedical model)
    {
        await _service.UpdateAsync(model);
        return RedirectToAction(nameof(Index), new { pacientId = model.PacientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int pacientId)
    {
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index), new { pacientId });
    }
}
