using System.Diagnostics;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

public class HomeController : Controller
{
    private readonly IResursaService _resurse;

    public HomeController(IResursaService resurse)
    {
        _resurse = resurse;
    }

    // Pagina de start — prezentare publică, accesibilă și fără autentificare.
    // Adminul (dacă e logat) vede în plus widget de alertă pentru revizii restante.
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
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
