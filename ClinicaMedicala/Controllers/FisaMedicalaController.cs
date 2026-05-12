using Microsoft.AspNetCore.Mvc;
using ClinicaMedicala.Models;

public class FisaMedicalaController : Controller
{
    private readonly IFisaMedicalaService _service;

    public FisaMedicalaController(IFisaMedicalaService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Details(int pacientId)
    {
        var fisa = await _service.GetByPacientId(pacientId);
        return View(fisa);
    }

    [HttpPost]
    public async Task<IActionResult> Save(FisaMedicala model)
    {
        await _service.CreateOrUpdate(model);
        return RedirectToAction("Details", new { pacientId = model.PacientId });
    }
}