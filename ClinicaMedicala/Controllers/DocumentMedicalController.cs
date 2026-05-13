using System.Security.Claims;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Pacienti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-38: încărcare documente externe (PDF, imagini).
// REQ-39: editare informații asociate.
// REQ-40: ștergere documente.
// REQ-41: asociere cu pacient.
[Authorize]
public class DocumentMedicalController : Controller
{
    private static readonly string[] ExtensiiPermise =
        new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };

    private const long DimensiuneMaxBytes = 10 * 1024 * 1024; // 10 MB

    private readonly IDocumentMedicalService _service;
    private readonly IWebHostEnvironment _env;

    public DocumentMedicalController(IDocumentMedicalService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    private int IdUtilizatorCurent() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool EsteStaff() =>
        User.IsInRole(Rol.Admin.ToString())
        || User.IsInRole(Rol.Medic.ToString())
        || User.IsInRole(Rol.Asistent.ToString());

    // ── LISTA documente pacient ───────────────────────────────────────────────
    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    public async Task<IActionResult> Index(int pacientId)
    {
        var docs = await _service.GetByPacientIdAsync(pacientId);
        ViewBag.PacientId = pacientId;
        return View(docs);
    }

    // ── UPLOAD fișier real (REQ-38) ───────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(15 * 1024 * 1024)] // 15 MB total request (includ overhead)
    public async Task<IActionResult> Upload(int pacientId, TipDocument tipDocument, string? observatii, IFormFile? fisier)
    {
        if (fisier == null || fisier.Length == 0)
        {
            TempData["Eroare"] = "Selectează un fișier de încărcat.";
            return RedirectToAction(nameof(Index), new { pacientId });
        }

        if (fisier.Length > DimensiuneMaxBytes)
        {
            TempData["Eroare"] = $"Fișierul depășește dimensiunea maximă de {DimensiuneMaxBytes / 1024 / 1024} MB.";
            return RedirectToAction(nameof(Index), new { pacientId });
        }

        var extensie = Path.GetExtension(fisier.FileName).ToLowerInvariant();
        if (!ExtensiiPermise.Contains(extensie))
        {
            TempData["Eroare"] = $"Tip de fișier nepermis. Acceptate: {string.Join(", ", ExtensiiPermise)}.";
            return RedirectToAction(nameof(Index), new { pacientId });
        }

        // Salvăm pe disc în wwwroot/uploads/documente/{pacientId}/
        var folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "documente", pacientId.ToString());
        Directory.CreateDirectory(folder);

        var numeUnic = $"{Guid.NewGuid():N}{extensie}";
        var caleAbsoluta = Path.Combine(folder, numeUnic);

        await using (var stream = new FileStream(caleAbsoluta, FileMode.Create))
        {
            await fisier.CopyToAsync(stream);
        }

        // Calea relativă stocată în DB (folosită la download)
        var caleRelativa = $"uploads/documente/{pacientId}/{numeUnic}";

        // Numele original păstrat în Observatii pentru context (opțional)
        var obsFinal = string.IsNullOrWhiteSpace(observatii)
            ? $"Fișier: {fisier.FileName}"
            : $"{observatii.Trim()} | Fișier: {fisier.FileName}";

        // MedicId — doar dacă utilizatorul curent e medic
        int? medicId = User.IsInRole(Rol.Medic.ToString()) ? IdUtilizatorCurent() : null;

        var doc = new DocumentMedical
        {
            PacientId = pacientId,
            TipDocument = tipDocument,
            CaleFisier = caleRelativa,
            Observatii = obsFinal,
            DataIncarcare = DateTime.UtcNow,
            MedicId = medicId
        };

        await _service.AddAsync(doc);

        TempData["Succes"] = "Documentul a fost încărcat cu succes.";
        return RedirectToAction(nameof(Index), new { pacientId });
    }

    // ── DOWNLOAD — pacientul propriu sau staff ───────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _service.GetByIdAsync(id);
        if (doc == null || string.IsNullOrWhiteSpace(doc.CaleFisier)) return NotFound();

        // Permisiuni: staff sau pacientul propriu
        if (!EsteStaff() && doc.PacientId != IdUtilizatorCurent()) return Forbid();

        var caleFizica = Path.Combine(_env.WebRootPath ?? "wwwroot",
            doc.CaleFisier.Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(caleFizica)) return NotFound();

        var ext = Path.GetExtension(caleFizica).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        var fileName = $"{doc.TipDocument}_{doc.DataIncarcare:yyyyMMdd}{ext}";
        return PhysicalFile(caleFizica, contentType, fileName);
    }

    // ── EDIT informații asociate (REQ-39) ─────────────────────────────────────
    [HttpGet]
    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _service.GetByIdAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    [HttpPost]
    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DocumentMedical model)
    {
        // Permitem doar editare info — nu și schimbarea fișierului (asta ar fi un upload nou)
        var existent = await _service.GetByIdAsync(model.Id);
        if (existent == null) return NotFound();

        existent.TipDocument = model.TipDocument;
        existent.Observatii = model.Observatii;
        await _service.UpdateAsync(existent);

        TempData["Succes"] = "Informațiile documentului au fost actualizate.";
        return RedirectToAction(nameof(Index), new { pacientId = existent.PacientId });
    }

    // ── ȘTERGERE (REQ-40) ─────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = PoliciiAuth.AdminSauMedic)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int pacientId)
    {
        var doc = await _service.GetByIdAsync(id);
        if (doc != null && !string.IsNullOrWhiteSpace(doc.CaleFisier))
        {
            var caleFizica = Path.Combine(_env.WebRootPath ?? "wwwroot",
                doc.CaleFisier.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(caleFizica))
            {
                try { System.IO.File.Delete(caleFizica); } catch { /* best-effort */ }
            }
        }

        await _service.DeleteAsync(id);
        TempData["Succes"] = "Documentul a fost șters.";
        return RedirectToAction(nameof(Index), new { pacientId });
    }
}
