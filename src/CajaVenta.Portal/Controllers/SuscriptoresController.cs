using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;
using CajaVenta.Portal.Models;
using CajaVenta.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Portal.Controllers;

[Authorize]
public class SuscriptoresController : Controller
{
    private readonly ISuscriptorService _suscriptores;
    private readonly ISuscripcionService _suscripciones;
    private readonly IPagoService _pagos;

    public SuscriptoresController(ISuscriptorService suscriptores, ISuscripcionService suscripciones, IPagoService pagos)
    {
        _suscriptores = suscriptores;
        _suscripciones = suscripciones;
        _pagos = pagos;
    }

    public async Task<IActionResult> Index()
    {
        var todos = await _suscriptores.ObtenerTodosAsync();
        return View(todos);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var suscriptor = await _suscriptores.ObtenerPorIdAsync(id);
        if (suscriptor is null)
            return NotFound();

        var model = new SuscriptorViewModel
        {
            Id = suscriptor.Id,
            NombreComercial = suscriptor.NombreComercial,
            RazonSocial = suscriptor.RazonSocial,
            Rfc = suscriptor.Rfc,
            Email = suscriptor.Email,
            Telefono = suscriptor.Telefono,
            UrlAcceso = suscriptor.UrlAcceso,
            Estado = new EstateSelect { Valor = suscriptor.Estado.ToString() },
            Suscripciones = suscriptor.Suscripciones.ToList(),
            Pagos = await _pagos.ObtenerPorSuscriptorAsync(id)
        };

        ViewData["RutaDb"] = suscriptor.RutaBaseDatos;
        return View(model);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new SuscriptorViewModel { Estado = new EstateSelect() });
    }

    [HttpPost]
    public async Task<IActionResult> Crear(SuscriptorViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NombreComercial))
            ModelState.AddModelError("NombreComercial", "El nombre comercial es obligatorio.");

        var estado = Enum.TryParse<EstadoSuscriptor>(model.Estado.Valor, out var e) ? e : EstadoSuscriptor.Activo;

        if (ModelState.IsValid)
        {
            var resultado = await _suscriptores.CrearAsync(
                model.NombreComercial, model.RazonSocial, model.Rfc, model.Email, model.Telefono,
                model.UrlAcceso, estado);

            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "Cliente creado y base de datos provisionada.";
                return RedirectToAction("Detalle", new { id = resultado.Id });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        model.Estado = new EstateSelect { Valor = model.Estado.Valor };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var suscriptor = await _suscriptores.ObtenerPorIdAsync(id);
        if (suscriptor is null)
            return NotFound();

        return View(new SuscriptorViewModel
        {
            Id = suscriptor.Id,
            NombreComercial = suscriptor.NombreComercial,
            RazonSocial = suscriptor.RazonSocial,
            Rfc = suscriptor.Rfc,
            Email = suscriptor.Email,
            Telefono = suscriptor.Telefono,
            UrlAcceso = suscriptor.UrlAcceso,
            Estado = new EstateSelect { Valor = suscriptor.Estado.ToString() }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(SuscriptorViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NombreComercial))
            ModelState.AddModelError("NombreComercial", "El nombre comercial es obligatorio.");

        var estado = Enum.TryParse<EstadoSuscriptor>(model.Estado.Valor, out var e) ? e : EstadoSuscriptor.Activo;

        if (ModelState.IsValid)
        {
            var resultado = await _suscriptores.ActualizarAsync(
                model.Id, model.NombreComercial, model.RazonSocial, model.Rfc, model.Email, model.Telefono,
                model.UrlAcceso, estado);

            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "Cliente actualizado.";
                return RedirectToAction("Detalle", new { id = model.Id });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        model.Estado = new EstateSelect { Valor = model.Estado.Valor };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(Guid id, string nuevoEstado)
    {
        var estado = Enum.TryParse<EstadoSuscriptor>(nuevoEstado, out var e) ? e : EstadoSuscriptor.Activo;
        var resultado = await _suscriptores.CambiarEstadoAsync(id, estado);
        TempData["Exito"] = resultado.EsExitoso ? "Estado actualizado." : resultado.Error;
        return RedirectToAction("Detalle", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var resultado = await _suscriptores.EliminarAsync(id);
        TempData["Exito"] = resultado.EsExitoso ? "Cliente eliminado." : resultado.Error;
        return RedirectToAction("Index");
    }
}