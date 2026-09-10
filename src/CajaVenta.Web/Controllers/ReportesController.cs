using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Common;
using CajaVenta.Domain.Enums;
using CajaVenta.Web.Models;
using CajaVenta.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
[Permiso(Permisos.VerReportes)]
public class ReportesController : Controller
{
    private readonly IVentaService _ventaService;
    private readonly IInventarioService _inventarioService;
    private readonly ISucursalService _sucursalService;

    public ReportesController(
        IVentaService ventaService,
        IInventarioService inventarioService,
        ISucursalService sucursalService)
    {
        _ventaService = ventaService;
        _inventarioService = inventarioService;
        _sucursalService = sucursalService;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, Guid? sucursalId)
    {
        var sucursal = await ResolverSucursalAsync(sucursalId);
        var hoy = DateTime.Today;
        var desdeFiltro = desde ?? hoy;
        var hastaFiltro = (hasta ?? hoy).AddDays(1).AddMilliseconds(-1);

        var ventasResultado = await _ventaService.ObtenerPorRangoFechasAsync(
            desdeFiltro.ToUniversalTime(),
            hastaFiltro.ToUniversalTime(),
            sucursal?.Id);
        var catalogoResultado = sucursal is null
            ? Result<List<StockProductoDto>>.Success(new List<StockProductoDto>())
            : await _inventarioService.ObtenerCatalogoStockAsync(sucursal.Id);

        var ventas = ventasResultado.IsSuccess ? ventasResultado.Value!.ToList() : new List<VentaDto>();

        var topCompleta = ventas.Where(v => v.Estado == EstadoVenta.Completada);
        var topProductos = topCompleta
            .SelectMany(v => v.Detalles)
            .GroupBy(d => d.ProductoNombre)
            .Select(g => new ResumenDetalle
            {
                Producto = g.Key,
                Cantidad = g.Sum(d => d.Cantidad),
                Importe = g.Sum(d => d.Total)
            })
            .OrderByDescending(r => r.Cantidad)
            .Take(10)
            .ToList();

        var porMetodo = topCompleta
            .GroupBy(v => v.MetodoPago)
            .Select(g => new ResumenMetodo
            {
                Metodo = g.Key,
                Importe = g.Sum(v => v.Total),
                Tickets = g.Count()
            })
            .OrderByDescending(r => r.Importe)
            .ToList();

        var stockBajo = (catalogoResultado.IsSuccess ? catalogoResultado.Value! : new List<StockProductoDto>())
            .Where(p => p.StockActual <= p.StockMinimo)
            .OrderBy(p => p.StockActual)
            .ToList();

        var sucursales = await _sucursalService.ObtenerTodosAsync(true);

        var modelo = new ReportesModel
        {
            SucursalId = sucursal?.Id ?? Guid.Empty,
            SucursalNombre = sucursal?.Nombre ?? "Todas las sucursales",
            Sucursales = sucursales.IsSuccess ? sucursales.Value! : new List<SucursalDto>(),
            Desde = desdeFiltro,
            Hasta = hastaFiltro.AddDays(-1),
            Ventas = ventas.OrderByDescending(v => v.FechaCreacion).ToList(),
            TopProductos = topProductos,
            PorMetodo = porMetodo,
            StockBajo = stockBajo
        };

        return View(modelo);
    }

    private async Task<SucursalDto?> ResolverSucursalAsync(Guid? sucursalId)
    {
        var claim = SucursalContext.ObtenerSucursalClaim(User);
        if (sucursalId.HasValue)
            return (await _sucursalService.ObtenerPorIdAsync(sucursalId.Value)).Value;
        if (claim.HasValue)
            return (await _sucursalService.ObtenerPorIdAsync(claim.Value)).Value;

        return null;
    }
}