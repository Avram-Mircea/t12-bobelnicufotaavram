using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Models.ViewModels;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Validare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Controllers;

// Unelte pentru testarea/diagnosticarea constrângerilor (REQ-17, REQ-18).
[Authorize(Policy = PoliciiAuth.DoarAdmin)]
public class ConstrangeriController : Controller
{
    private readonly IConstraintValidationService _validator;
    private readonly ApplicationDbContext _ctx;

    public ConstrangeriController(IConstraintValidationService validator, ApplicationDbContext ctx)
    {
        _validator = validator;
        _ctx = ctx;
    }

    [HttpGet]
    public async Task<IActionResult> TestValidator()
    {
        var model = await PopuleazaDropdownsAsync(new TestValidatorViewModel());
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestValidator(TestValidatorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopuleazaDropdownsAsync(model);
            return View(model);
        }

        var cerere = new CerereValidareProgramare
        {
            MedicId = model.MedicId!.Value,
            TipProgramare = model.TipProgramare,
            AsistentId = model.AsistentId,
            ResursaIds = model.ResursaIds,
            DataStart = model.DataStart,
            DataEnd = model.DataEnd
        };

        var rezultat = await _validator.ValideazaProgramareAsync(cerere);
        ViewBag.Rezultat = rezultat;

        await PopuleazaDropdownsAsync(model);
        return View(model);
    }

    private async Task<TestValidatorViewModel> PopuleazaDropdownsAsync(TestValidatorViewModel model)
    {
        model.Medici = await _ctx.Medici
            .Where(m => m.StatusCont)
            .OrderBy(m => m.Nume).ToListAsync();

        model.Asistenti = await _ctx.Asistenti
            .Where(a => a.StatusCont)
            .OrderBy(a => a.Nume).ToListAsync();

        model.Resurse = await _ctx.Resurse
            .Include(r => r.Specializari)
            .OrderBy(r => r.Tip).ThenBy(r => r.Denumire)
            .ToListAsync();

        return model;
    }
}
