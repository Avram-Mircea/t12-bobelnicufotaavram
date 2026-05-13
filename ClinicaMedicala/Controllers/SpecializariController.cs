using System.ComponentModel.DataAnnotations;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// Management specializări medicale — folosite la asocierea cu resursele (REQ-13)
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class SpecializariController : Controller
{
    private readonly ISpecializareService _service;

    public SpecializariController(ISpecializareService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _service.GetAllAsync();
        return View(lista);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreareSpecializareForm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreareSpecializareForm form)
    {
        if (!ModelState.IsValid) return View(form);

        try
        {
            await _service.CreeazaAsync(form.Nume, form.Descriere);
            TempData["Succes"] = $"Specializarea „{form.Nume}” a fost adăugată.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(form);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActiv(int id)
    {
        var ok = await _service.ToggleActivAsync(id);
        TempData[ok ? "Succes" : "Eroare"] = ok
            ? "Starea specializării a fost schimbată."
            : "Specializarea nu a putut fi modificată.";
        return RedirectToAction(nameof(Index));
    }

    public class CreareSpecializareForm
    {
        [Required(ErrorMessage = "Numele este obligatoriu.")]
        [MaxLength(100)]
        public string Nume { get; set; } = null!;

        [MaxLength(500)]
        public string? Descriere { get; set; }
    }
}
