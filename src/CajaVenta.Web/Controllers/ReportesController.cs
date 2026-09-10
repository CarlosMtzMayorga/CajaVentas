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
    private readonly ICorteZService _corteZService;

    public ReportesController(
        IVentaService ventaService,
        IInventarioService inventarioService,
        ISucursalService sucursalService,
        ICorteZService corteZService)
    {
        _ventaService = ventaService;
        _inventarioService = inventarioService;
        _sucursalService = sucursalService;
        _corteZService = corteZService;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, Guid? sucursalId, Guid? cajaId)
    {
        var sucursal = await ResolverSucursalAsync(sucursalId);
        var hoy = DateTime.Today;
        var desdeFiltro = desde ?? hoy;
        var hastaFiltro = (hasta ?? hoy).AddDays(1).AddMilliseconds(-1);

        var caja = await ResolverCajaAsync(sucursal?.Id, cajaId);

        var ventasResultado = await _ventaService.ObtenerPorRangoFechasAsync(
            desdeFiltro.ToUniversalTime(),
            hastaFiltro.ToUniversalTime(),
            sucursal?.Id,
            caja?.Id);
        var cortesZResultado = await _corteZService.ObtenerPorRangoFechasAsync(
            desdeFiltro.ToUniversalTime(),
            hastaFiltro.ToUniversalTime(),
            sucursal?.Id,
            caja?.Id);
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
        var cajas = await ObtenerCajasAsync(sucursal?.Id);

        var modelo = new ReportesModel
        {
            SucursalId = sucursal?.Id ?? Guid.Empty,
            SucursalNombre = sucursal?.Nombre ?? "Todas las sucursales",
            Sucursales = sucursales.IsSuccess ? sucursales.Value! : new List<SucursalDto>(),
            CajaId = caja?.Id ?? Guid.Empty,
            CajaNombre = caja?.Nombre ?? "Todas las cajas",
            Cajas = cajas,
            Desde = desdeFiltro,
            Hasta = hastaFiltro.AddDays(-1),
            Ventas = ventas.OrderByDescending(v => v.FechaCreacion).ToList(),
            CortesZ = cortesZResultado.IsSuccess ? cortesZResultado.Value! : new List<CorteZDto>(),
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

    private async Task<CajaDto?> ResolverCajaAsync(Guid? sucursalId, Guid? cajaId)
    {
        if (!cajaId.HasValue)
            return null;

        if (sucursalId.HasValue)
        {
            var resultado = await _sucursalService.ObtenerCajasAsync(sucursalId.Value);
            return resultado.IsSuccess ? resultado.Value!.FirstOrDefault(c => c.Id == cajaId.Value) : null;
        }

        var sucursales = await _sucursalService.ObtenerTodosAsync(true);
        if (sucursales.IsSuccess)
        {
            foreach (var s in sucursales.Value!)
            {
                var resultado = await _sucursalService.ObtenerCajasAsync(s.Id);
                if (resultado.IsSuccess)
                {
                    var caja = resultado.Value!.FirstOrDefault(c => c.Id == cajaId.Value);
                    if (caja is not null)
                        return caja;
                }
            }
        }

        return null;
    }

    private async Task<List<CajaDto>> ObtenerCajasAsync(Guid? sucursalId)
    {
        var lista = new List<CajaDto>();
        if (sucursalId.HasValue)
        {
            var resultado = await _sucursalService.ObtenerCajasAsync(sucursalId.Value);
            return resultado.IsSuccess ? resultado.Value!.Where(c => c.Activo).ToList() : lista;
        }

        var sucursales = await _sucursalService.ObtenerTodosAsync(true);
        if (sucursales.IsSuccess)
        {
            foreach (var s in sucursales.Value!)
            {
                var resultado = await _sucursalService.ObtenerCajasAsync(s.Id);
                if (resultado.IsSuccess)
                    lista.AddRange(resultado.Value!.Where(c => c.Activo));
            }
        }

        return lista;
    }
}