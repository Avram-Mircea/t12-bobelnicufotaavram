using System.Diagnostics;
using ClinicaMedicala.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaMedicala.Controllers;

// REQ-05: pagina principală cere autentificare. Auth/Login etc. rămân publice.
[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
