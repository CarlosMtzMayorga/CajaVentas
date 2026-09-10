using CajaVenta.Portal.Enums;
using CajaVenta.Portal.Models;
using CajaVenta.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Portal.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ISuscriptorService _suscriptores;
    private readonly ISuscripcionService _suscripciones;
    private readonly IPagoService _pagos;

    public DashboardController(ISuscriptorService suscriptores, ISuscripcionService suscripciones, IPagoService pagos)
    {
        _suscriptores = suscriptores;
        _suscripciones = suscripciones;
        _pagos = pagos;
    }

    public async Task<IActionResult> Index()
    {
        var todos = await _suscriptores.ObtenerTodosAsync();
        var suscripciones = await _suscripciones.ObtenerTodasAsync();
        var vencidas = suscripciones
            .Where(s => s.Estado == EstadoSuscripcion.Activa && s.FechaVencimiento < DateTime.UtcNow)
            .ToList();
        var porVencer = suscripciones
            .Where(s => s.Estado == EstadoSuscripcion.Activa
                        && s.FechaVencimiento >= DateTime.UtcNow
                        && s.FechaVencimiento <= DateTime.UtcNow.AddDays(7))
            .OrderBy(s => s.FechaVencimiento)
            .ToList();
        var pagos = await _pagos.ObtenerTodosAsync();

        var model = new DashboardViewModel
        {
            TotalSuscriptores = todos.Count,
            Activos = todos.Count(s => s.Estado == EstadoSuscriptor.Activo),
            Suspendidos = todos.Count(s => s.Estado == EstadoSuscriptor.Suspendido),
            Cancelados = todos.Count(s => s.Estado == EstadoSuscriptor.Cancelado),
            EnPrueba = todos.Count(s => s.Estado == EstadoSuscriptor.Prueba),
            SuscripcionesActivas = suscripciones.Count(s => s.Estado == EstadoSuscripcion.Activa),
            SuscripcionesVencidas = vencidas.Count,
            IngresosMes = await _pagos.ObtenerIngresosDelMesAsync(),
            IngresosTotales = await _pagos.ObtenerIngresosTotalesAsync(),
            PorVencer = porVencer,
            UltimosPagos = pagos.Take(8).ToList()
        };

        return View(model);
    }
}