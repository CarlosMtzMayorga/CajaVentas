using System.Security.Claims;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Common;
using CajaVenta.Web.Models;
using CajaVenta.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
[Permiso(Permisos.Turnos)]
public class TurnosController : Controller
{
    private readonly ITurnoService _turnoService;
    private readonly ISucursalService _sucursalService;

    public TurnosController(ITurnoService turnoService, ISucursalService sucursalService)
    {
        _turnoService = turnoService;
        _sucursalService = sucursalService;
    }

    private Guid UsuarioId
        => Guid.Parse(User.FindFirstValue("UsuarioId")!);

    public async Task<IActionResult> Index()
    {
        var turnoResultado = await _turnoService.ObtenerTurnoAbiertoAsync(UsuarioId);
        var historial = await _turnoService.ObtenerHistorialAsync(DateTime.Today.AddDays(-30), DateTime.UtcNow);

        var (cajas, sucursalNombre) = await ObtenerCajasDisponiblesAsync();

        var modelo = new TurnosIndexModel
        {
            TurnoAbierto = turnoResultado.IsSuccess ? turnoResultado.Value : null,
            Historial = historial.IsSuccess ? historial.Value!.OrderByDescending(h => h.FechaApertura).ToList() : new List<TurnoCajaDto>(),
            Cajas = cajas,
            SucursalNombre = sucursalNombre,
            Mensaje = TempData["Mensaje"] as string,
            Error = TempData["Error"] as string
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(Guid cajaId, decimal fondoInicial, string? observaciones)
    {
        if (fondoInicial < 0)
        {
            TempData["Error"] = "El fondo inicial no puede ser negativo";
            return RedirectToAction(nameof(Index));
        }

        if (cajaId == Guid.Empty)
        {
            TempData["Error"] = "Selecciona una caja";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _turnoService.AbrirTurnoAsync(new AbrirTurnoDto
        {
            UsuarioId = UsuarioId,
            CajaId = cajaId,
            FondoInicial = fondoInicial,
            Observaciones = observaciones
        });

        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = $"Turno abierto en {resultado.Value!.CajaNombre} ({resultado.Value.SucursalNombre}) con fondo de ${resultado.Value!.FondoInicial:N2}";

        return RedirectToAction(nameof(Index));
    }

    private async Task<(List<CajaDto> Cajas, string? SucursalNombre)> ObtenerCajasDisponiblesAsync()
    {
        var claim = SucursalContext.ObtenerSucursalClaim(User);
        if (claim.HasValue)
        {
            var cajas = await _sucursalService.ObtenerCajasAsync(claim.Value);
            string? nombre = null;
            var sucursales = await _sucursalService.ObtenerTodosAsync(true);
            nombre = sucursales.IsSuccess
                ? sucursales.Value!.FirstOrDefault(s => s.Id == claim.Value)?.Nombre
                : null;
            return (cajas.IsSuccess ? cajas.Value! : new List<CajaDto>(), nombre);
        }

        var lista = new List<CajaDto>();
        var sucs = await _sucursalService.ObtenerTodosAsync(true);
        if (sucs.IsSuccess)
        {
            foreach (var s in sucs.Value!)
            {
                var cjs = await _sucursalService.ObtenerCajasAsync(s.Id);
                if (cjs.IsSuccess)
                    lista.AddRange(cjs.Value!);
            }
        }
        return (lista, null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(decimal fondoFinal, string? observaciones)
    {
        var turno = await _turnoService.ObtenerTurnoAbiertoAsync(UsuarioId);
        if (turno.IsFailure)
        {
            TempData["Error"] = turno.Error;
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _turnoService.CerrarTurnoAsync(turno.Value!.Id, new CerrarTurnoDto
        {
            FondoFinal = fondoFinal,
            Observaciones = observaciones
        });

        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = "Turno cerrado correctamente";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImprimirCorte(Guid turnoId)
    {
        var resultado = await _turnoService.GenerarCorteXAsync(turnoId);
        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        TempData["CorteX"] = System.Text.Json.JsonSerializer.Serialize(resultado.Value);
        return RedirectToAction(nameof(Corte), new { turnoId });
    }

    public IActionResult Corte(Guid turnoId)
    {
        var json = TempData["CorteX"] as string;
        if (string.IsNullOrWhiteSpace(json))
            return RedirectToAction(nameof(Index));

        var corte = System.Text.Json.JsonSerializer.Deserialize<CorteXDto>(json);
        if (corte is null || corte.TurnoCajaId != turnoId)
            return RedirectToAction(nameof(Index));

        return View(corte);
    }
}