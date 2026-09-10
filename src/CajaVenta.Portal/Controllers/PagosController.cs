using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;
using CajaVenta.Portal.Models;
using CajaVenta.Portal.Services;
using CajaVenta.Portal.Services.Pasarelas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Controllers;

[Authorize]
public class PagosController : Controller
{
    private readonly IPagoService _pagos;
    private readonly ISuscriptorService _suscriptores;
    private readonly ISuscripcionService _suscripciones;
    private readonly IPasarelaPago _pasarela;

    public PagosController(IPagoService pagos, ISuscriptorService suscriptores, ISuscripcionService suscripciones, IPasarelaPago pasarela)
    {
        _pagos = pagos;
        _suscriptores = suscriptores;
        _suscripciones = suscripciones;
        _pasarela = pasarela;
    }

    public async Task<IActionResult> Index()
    {
        var pagos = await _pagos.ObtenerTodosAsync();
        return View(pagos);
    }

    [HttpGet]
    public async Task<IActionResult> Registrar(Guid? suscriptorId, Guid? suscripcionId)
    {
        return View(new PagoViewModel
        {
            Suscriptores = await _suscriptores.ObtenerTodosAsync(),
            Suscripciones = await ObtenerSuscripcionesActivasAsync(suscriptorId),
            SuscriptorId = suscriptorId ?? Guid.Empty,
            SuscripcionId = suscripcionId ?? Guid.Empty
        });
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(PagoViewModel model)
    {
        model.Suscriptores = await _suscriptores.ObtenerTodosAsync();
        model.Suscripciones = await ObtenerSuscripcionesActivasAsync(model.SuscriptorId);

        if (model.SuscripcionId == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Selecciona una suscripción.");
            return View(model);
        }

        if (ModelState.IsValid)
        {
            var resultado = await _pagos.RegistrarAsync(model.SuscriptorId, model.SuscripcionId, model.Monto, model.Metodo, model.Notas);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "Pago registrado.";
                return RedirectToAction("Detalle", "Suscriptores", new { id = model.SuscriptorId });
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public IActionResult SimularPago(string referencia, decimal monto, string suscripcionId)
    {
        ViewBag.Referencia = referencia;
        ViewBag.Monto = monto;
        ViewBag.SuscripcionId = suscripcionId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmarPagoSimulado(string suscripcionId)
    {
        if (!Guid.TryParse(suscripcionId, out var id))
        {
            TempData["Error"] = "Referencia de suscripción no válida.";
            return RedirectToAction("Index");
        }

        var suscripcion = await _suscripciones.ObtenerPorIdAsync(id);
        if (suscripcion is null)
        {
            TempData["Error"] = "Suscripción no encontrada.";
            return RedirectToAction("Index");
        }

        var resultado = await _pagos.RegistrarAsync(
            suscripcion.SuscriptorId, suscripcion.Id, suscripcion.PrecioMensual,
            MetodoPagoPasarela.Tarjeta, "Pago simulado (demo)");

        TempData["Exito"] = resultado.EsExitoso ? "Pago confirmado y suscripción activada." : resultado.Error;
        return RedirectToAction("Detalle", "Suscriptores", new { id = suscripcion.SuscriptorId });
    }

    private async Task<List<Suscripcion>> ObtenerSuscripcionesActivasAsync(Guid? suscriptorId)
    {
        if (suscriptorId.HasValue && suscriptorId != Guid.Empty)
            return await _suscripciones.ObtenerPorSuscriptorAsync(suscriptorId.Value);

        var todas = await _suscripciones.ObtenerTodasAsync();
        return todas;
    }
}