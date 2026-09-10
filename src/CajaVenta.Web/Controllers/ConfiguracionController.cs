using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Common;
using CajaVenta.Web.Models;
using CajaVenta.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
[Permiso(Permisos.Configuracion)]
public class ConfiguracionController : Controller
{
    private readonly IPermisoService _permisoService;

    public ConfiguracionController(IPermisoService permisoService)
    {
        _permisoService = permisoService;
    }

    public IActionResult Index() => View();

    [HttpGet]
    [Permiso(Permisos.Permisologia)]
    public async Task<IActionResult> Permisologia()
    {
        var admin = await _permisoService.ObtenerPermisosRolAsync("Admin");
        var cajero = await _permisoService.ObtenerPermisosRolAsync("Cajero");

        var model = new PermisologiaModel
        {
            Roles =
            [
                new RolPermisosRow { Rol = "Admin", PermisosActivos = admin.ToHashSet(StringComparer.OrdinalIgnoreCase) },
                new RolPermisosRow { Rol = "Cajero", PermisosActivos = cajero.ToHashSet(StringComparer.OrdinalIgnoreCase) }
            ]
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permiso(Permisos.Permisologia)]
    public async Task<IActionResult> Permisologia(string? rol, string[]? permisos)
    {
        if (string.IsNullOrWhiteSpace(rol))
        {
            TempData["Error"] = "No se indicó el rol a modificar.";
            return RedirectToAction(nameof(Permisologia));
        }

        var ok = await _permisoService.ActualizarPermisosAsync(rol, permisos ?? []);
        if (!ok)
        {
            TempData["Error"] = "No se pueden modificar los permisos de este rol.";
            return RedirectToAction(nameof(Permisologia));
        }

        TempData["Mensaje"] = $"Permisos de {rol} actualizados correctamente.";
        return RedirectToAction(nameof(Permisologia));
    }
}