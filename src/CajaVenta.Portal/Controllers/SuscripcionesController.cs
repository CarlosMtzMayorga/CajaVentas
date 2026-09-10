using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;
using CajaVenta.Portal.Models;
using CajaVenta.Portal.Services;
using CajaVenta.Portal.Services.Pasarelas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Portal.Controllers;

[Authorize]
public class SuscripcionesController : Controller
{
    private readonly ISuscripcionService _suscripciones;
    private readonly ISuscriptorService _suscriptores;
    private readonly IPlanService _planes;
    private readonly IPagoService _pagos;
    private readonly IPasarelaPago _pasarela;

    public SuscripcionesController(
        ISuscripcionService suscripciones,
        ISuscriptorService suscriptores,
        IPlanService planes,
        IPagoService pagos,
        IPasarelaPago pasarela)
    {
        _suscripciones = suscripciones;
        _suscriptores = suscriptores;
        _planes = planes;
        _pagos = pagos;
        _pasarela = pasarela;
    }

    public async Task<IActionResult> Index()
    {
        var todas = await _suscripciones.ObtenerTodasAsync();
        return View(todas);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        return View(new SuscripcionViewModel
        {
            Suscriptores = await _suscriptores.ObtenerTodosAsync(),
            Planes = await _planes.ObtenerActivosAsync(),
            FechaInicio = DateTime.UtcNow.Date
        });
    }

    [HttpPost]
    public async Task<IActionResult> Crear(SuscripcionViewModel model)
    {
        var suscriptores = await _suscriptores.ObtenerTodosAsync();
        var planes = await _planes.ObtenerActivosAsync();
        model.Suscriptores = suscriptores;
        model.Planes = planes;

        if (model.SuscriptorId == Guid.Empty || model.PlanId == Guid.Empty)
        {
            ModelState.AddModelError(string.Empty, "Selecciona cliente y plan.");
            return View(model);
        }

        if (model.Meses < 1 || model.Meses > 36)
            ModelState.AddModelError("Meses", "Los meses deben estar entre 1 y 36.");

        var plan = planes.FirstOrDefault(p => p.Id == model.PlanId);
        if (plan is null)
        {
            ModelState.AddModelError(string.Empty, "Plan no válido.");
            return View(model);
        }

        if (ModelState.IsValid)
        {
            var resultado = await _suscripciones.CrearAsync(
                model.SuscriptorId, model.PlanId, plan.PrecioMensual, model.FechaInicio, model.Meses,
                model.Estado);

            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "Suscripción creada.";
                return RedirectToAction("Detalle", "Suscriptores", new { id = model.SuscriptorId });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var suscripcion = await _suscripciones.ObtenerPorIdAsync(id);
        if (suscripcion is null)
            return NotFound();

        return View(new SuscripcionViewModel
        {
            Id = suscripcion.Id,
            SuscriptorId = suscripcion.SuscriptorId,
            PlanId = suscripcion.PlanId,
            PrecioMensual = suscripcion.PrecioMensual,
            FechaInicio = suscripcion.FechaInicio,
            FechaVencimiento = suscripcion.FechaVencimiento,
            Estado = suscripcion.Estado,
            Suscriptores = await _suscriptores.ObtenerTodosAsync(),
            Planes = await _planes.ObtenerTodosAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(SuscripcionViewModel model)
    {
        var suscriptores = await _suscriptores.ObtenerTodosAsync();
        var planes = await _planes.ObtenerTodosAsync();
        model.Suscriptores = suscriptores;
        model.Planes = planes;

        if (ModelState.IsValid)
        {
            var resultado = await _suscripciones.ActualizarAsync(
                model.Id, model.PlanId, model.PrecioMensual, model.FechaInicio, model.FechaVencimiento,
                model.Estado);
            if (resultado.EsExitoso)
            {
                TempData["Exito"] = "Suscripción actualizada.";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var resultado = await _suscripciones.CancelarAsync(id);
        TempData["Exito"] = resultado.EsExitoso ? "Suscripción cancelada." : resultado.Error;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Activar(Guid id)
    {
        var resultado = await _suscripciones.ActivarAsync(id);
        TempData["Exito"] = resultado.EsExitoso ? "Suscripción activada." : resultado.Error;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarcarVencida(Guid id)
    {
        var resultado = await _suscripciones.MarcarVencidaAsync(id);
        TempData["Exito"] = resultado.EsExitoso ? "Suscripción marcada como vencida." : resultado.Error;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Cobrar(Guid id)
    {
        var suscripcion = await _suscripciones.ObtenerPorIdAsync(id);
        if (suscripcion is null)
            return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var datosCobro = new DatosCobro(
            suscripcion.Id,
            suscripcion.SuscriptorId,
            suscripcion.PrecioMensual,
            $"Suscripción {suscripcion.Plan?.Nombre} - {suscripcion.Suscriptor?.NombreComercial}",
            suscripcion.Suscriptor?.Email,
            $"{baseUrl}/Suscripciones/PagoExitoso?suscripcionId={suscripcion.Id}",
            $"{baseUrl}/Suscripciones/Index");

        try
        {
            var resultadoCobro = await _pasarela.CrearCobroAsync(datosCobro);
            return Redirect(resultadoCobro.UrlPago);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"No se pudo iniciar el cobro: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> PagoExitoso(string suscripcionId)
    {
        if (Guid.TryParse(suscripcionId, out var id))
        {
            var suscripcion = await _suscripciones.ObtenerPorIdAsync(id);
            if (suscripcion is not null)
            {
                await _pagos.RegistrarAsync(
                    suscripcion.SuscriptorId, suscripcion.Id, suscripcion.PrecioMensual,
                    ObtenerMetodo(), "Cobro vía pasarela (retorno de éxito)");
                TempData["Exito"] = "Pago registrado y suscripción activada.";
                return RedirectToAction("Detalle", "Suscriptores", new { id = suscripcion.SuscriptorId });
            }
        }

        TempData["Error"] = "No se pudo procesar la confirmación del pago.";
        return RedirectToAction("Index");
    }

    private MetodoPagoPasarela ObtenerMetodo() => _pasarela.Nombre switch
    {
        "Stripe" => MetodoPagoPasarela.Stripe,
        "MercadoPago" => MetodoPagoPasarela.MercadoPago,
        _ => MetodoPagoPasarela.Tarjeta
    };
}