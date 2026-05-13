using System.Diagnostics;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-05: pagina principală cere autentificare (excepție: Privacy/Error).
[Authorize]
public class HomeController : Controller
{
    private readonly IResursaService _resurse;

    public HomeController(IResursaService resurse)
    {
        _resurse = resurse;
    }

    public async Task<IActionResult> Index()
    {
        // Redirect rolul utilizatorului către dashboard-ul specific
        if (User.IsInRole(Rol.Pacient.ToString()))
            return RedirectToAction("Index", "Pacient");

        if (User.IsInRole(Rol.Medic.ToString()))
            return RedirectToAction("Index", "Medic");

        if (User.IsInRole(Rol.Asistent.ToString()))
            return RedirectToAction("Index", "Programare");

        // Pentru admin: dashboard local cu widget-ul de resurse cu revizie restantă
        if (User.IsInRole(Rol.Admin.ToString()))
        {
            ViewBag.ResurseCuRevizieRestanta = await _resurse.NumarCuRevizieRestantaAsync();
        }

        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
