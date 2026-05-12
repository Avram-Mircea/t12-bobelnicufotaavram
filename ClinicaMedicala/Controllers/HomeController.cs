using System.Diagnostics;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-05: pagina principală cere autentificare. Auth/Login etc. rămân publice.
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
        // Dashboard widget pentru admin: resurse cu revizie restantă
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
