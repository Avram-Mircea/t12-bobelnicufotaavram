using ClinicaMedicala.Models;
using ClinicaMedicala.Services;
using ClinicaMedicala.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace ClinicaMedicala.Controllers;

public class ProgramareController : Controller
{
    private readonly IProgramareService _programareService;
    private readonly IGenericService<Medic> _medicService;
    private readonly IGenericService<Pacient> _pacientService;
    private readonly IGenericService<Resursa> _resursaService;

    public ProgramareController(
        IProgramareService programareService,
        IGenericService<Medic> medicService,
        IGenericService<Pacient> pacientService,
        IGenericService<Resursa> resursaService)
    {
        _programareService = programareService;
        _medicService = medicService;
        _pacientService = pacientService;
        _resursaService = resursaService;
    }

    public async Task<IActionResult> Index()
    {
        var programari = await _programareService.GetAllAsync();
        return View(programari);
    }

    public async Task<IActionResult> Calendar()
    {
        // Pagină ce va conține calendarul (ex: FullCalendar)
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulareDictionare();
        return View(new ProgramareCreateViewModel { DataStart = DateTime.Now, DataEnd = DateTime.Now.AddMinutes(30) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProgramareCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Validare constrângeri de disponibilitate medic
            bool medicDisponibil = await _programareService.MedicEsteDisponibilAsync(model.MedicId, model.DataStart, model.DataEnd);
            if (!medicDisponibil)
            {
                ModelState.AddModelError("", "Medicul ales nu este disponibil în acest interval.");
            }

            // Validare constrângeri resurse
            if (model.ResursaId.HasValue)
            {
                bool resursaDisponibila = await _programareService.ResursaEsteDisponibilaAsync(model.ResursaId.Value, model.DataStart, model.DataEnd);
                if (!resursaDisponibila)
                {
                    ModelState.AddModelError("", "Resursa (sala/aparatul) aleasă nu este disponibilă.");
                }
            }

            if (model.DataEnd <= model.DataStart)
            {
                ModelState.AddModelError("DataEnd", "Data de sfârșit trebuie să fie după data de început.");
            }

            if (ModelState.ErrorCount == 0)
            {
                var programare = new Programare
                {
                    DataStart = model.DataStart,
                    DataEnd = model.DataEnd,
                    MotivVizita = model.MotivVizita,
                    TipProgramare = model.TipProgramare,
                    PacientId = model.PacientId,
                    MedicId = model.MedicId,
                    ResursaId = model.ResursaId,
                    DataCreare = DateTime.UtcNow,
                    Status = StatusProgramare.Programat
                };

                await _programareService.AddAsync(programare);
                await _programareService.SaveChangesAsync();

                // TODO: Notificare trimitere (REQ-26)

                return RedirectToAction(nameof(Index));
            }
        }

        await PopulareDictionare();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetProgramariForCalendar(string start, string end, int? medicId, int? resursaId)
    {
        if (DateTime.TryParse(start, out DateTime startDate) && DateTime.TryParse(end, out DateTime endDate))
        {
            var programari = await _programareService.ObtineProgramariPentruCalendarAsync(startDate, endDate, medicId, resursaId);
            var events = programari.Select(p => new
            {
                id = p.Id,
                title = $"{p.Pacient?.Nume} {p.Pacient?.Prenume} - {p.MotivVizita}",
                start = p.DataStart.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = p.DataEnd.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = medicId.HasValue ? "#3788d8" : "#28a745", // colorare opțională
                url = Url.Action("Details", "Programare", new { id = p.Id })
            });

            return Json(events);
        }

        return BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null)
            return NotFound();

        var model = new ProgramareCreateViewModel
        {
            Id = programare.Id,
            DataStart = programare.DataStart,
            DataEnd = programare.DataEnd,
            MotivVizita = programare.MotivVizita,
            TipProgramare = programare.TipProgramare,
            PacientId = programare.PacientId,
            MedicId = programare.MedicId,
            ResursaId = programare.ResursaId
        };

        await PopulareDictionare();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProgramareCreateViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (ModelState.IsValid)
        {
            // Validare disponibilitate
            bool medicDisponibil = await _programareService.MedicEsteDisponibilAsync(model.MedicId, model.DataStart, model.DataEnd, id);
            if (!medicDisponibil)
                ModelState.AddModelError("", "Medicul ales nu este disponibil în acest interval.");

            if (model.ResursaId.HasValue)
            {
                bool resursaDisponibila = await _programareService.ResursaEsteDisponibilaAsync(model.ResursaId.Value, model.DataStart, model.DataEnd, id);
                if (!resursaDisponibila)
                    ModelState.AddModelError("", "Resursa aleasă nu este disponibilă.");
            }

            if (model.DataEnd <= model.DataStart)
                ModelState.AddModelError("DataEnd", "Data de sfârșit trebuie să fie după data de început.");

            if (ModelState.ErrorCount == 0)
            {
                var programare = await _programareService.GetByIdAsync(id);
                if (programare == null)
                    return NotFound();

                programare.DataStart = model.DataStart;
                programare.DataEnd = model.DataEnd;
                programare.MotivVizita = model.MotivVizita;
                programare.TipProgramare = model.TipProgramare;
                programare.PacientId = model.PacientId;
                programare.MedicId = model.MedicId;
                programare.ResursaId = model.ResursaId;

                _programareService.Update(programare);
                await _programareService.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }

        await PopulareDictionare();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string motivAnulare)
    {
        var programare = await _programareService.GetByIdAsync(id);
        if (programare == null)
            return NotFound();

        programare.Status = StatusProgramare.Anulat_Clinica;
        programare.MotivAnulare = motivAnulare;

        _programareService.Update(programare);
        await _programareService.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulareDictionare()
    {
        ViewBag.Pacienti = new SelectList(await _pacientService.GetAllAsync(), "Id", "Nume");
        ViewBag.Medici = new SelectList(await _medicService.GetAllAsync(), "Id", "Nume");
        ViewBag.Resurse = new SelectList(await _resursaService.GetAllAsync(), "Id", "Denumire");
    }
}