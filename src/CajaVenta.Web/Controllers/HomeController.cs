using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
        => RedirectToAction("Index", "Pos");

    [AllowAnonymous]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        ViewData["MensajeError"] = feature?.Error?.Message ?? "Ocurrió un error inesperado";
        return View();
    }
}