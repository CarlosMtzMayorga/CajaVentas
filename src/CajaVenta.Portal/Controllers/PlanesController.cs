using CajaVenta.Portal.Models;
using CajaVenta.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CajaVenta.Portal.Controllers;

[Authorize]
public class PlanesController : Controller
{
    private readonly IPlanService _planes;

    public PlanesController(IPlanService planes) => _planes = planes;

    public async Task<IActionResult> Index()
    {
        var planes = await _planes.ObtenerTodosAsync();
        return View(planes.Select(p => new PlanViewModel
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            PrecioMensual = p.PrecioMensual,
            Activo = p.Activo
        }).ToList());
    }

    [HttpGet]
    public IActionResult Crear() => View(new PlanViewModel());

    [HttpPost]
    public async Task<IActionResult> Crear(PlanViewModel model)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _planes.CrearAsync(model.Nombre, model.Descripcion, model.PrecioMensual, model.Activo);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "Plan creado.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var plan = await _planes.ObtenerPorIdAsync(id);
        if (plan is null)
            return NotFound();

        return View(new PlanViewModel
        {
            Id = plan.Id,
            Nombre = plan.Nombre,
            Descripcion = plan.Descripcion,
            PrecioMensual = plan.PrecioMensual,
            Activo = plan.Activo
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(PlanViewModel model)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _planes.ActualizarAsync(model.Id, model.Nombre, model.Descripcion, model.PrecioMensual, model.Activo);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "Plan actualizado.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var resultado = await _planes.EliminarAsync(id);
        TempData["Exito"] = resultado.EsExitoso ? "Plan eliminado." : resultado.Error;
        return RedirectToAction("Index");
    }
}