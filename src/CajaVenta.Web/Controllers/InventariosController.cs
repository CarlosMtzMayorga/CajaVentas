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
[Permiso(Permisos.VerInventarios)]
public class InventariosController : Controller
{
    private readonly IInventarioService _inventarioService;
    private readonly IProductoService _productoService;
    private readonly ISucursalService _sucursalService;

    public InventariosController(
        IInventarioService inventarioService,
        IProductoService productoService,
        ISucursalService sucursalService)
    {
        _inventarioService = inventarioService;
        _productoService = productoService;
        _sucursalService = sucursalService;
    }

    public async Task<IActionResult> Index(Guid? sucursalId)
    {
        var sucursal = await ResolverSucursalAsync(sucursalId);
        if (sucursal is null)
        {
            TempData["Error"] = "No hay sucursales activas";
            return RedirectToAction("Index", "Home");
        }

        var resultado = await _inventarioService.ObtenerCatalogoStockAsync(sucursal.Id);

        var stock = (resultado.IsSuccess ? resultado.Value! : new List<StockProductoDto>())
            .OrderBy(p => p.Nombre)
            .Select(p => new StockItem
            {
                ProductoId = p.ProductoId,
                CodigoBarras = p.CodigoBarras,
                Nombre = p.Nombre,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                Costo = p.Costo,
                PrecioVenta = p.PrecioVenta
            })
            .ToList();

        var sucursales = await _sucursalService.ObtenerTodosAsync(true);

        var modelo = new InventarioResumenModel
        {
            SucursalId = sucursal.Id,
            SucursalNombre = sucursal.Nombre,
            Sucursales = sucursales.IsSuccess ? sucursales.Value! : new List<SucursalDto>(),
            Stock = stock,
            Mensaje = TempData["Mensaje"] as string,
            Error = TempData["Error"] as string
        };
        return View(modelo);
    }

    public async Task<IActionResult> Movimientos(
        Guid? sucursalId,
        DateTime? desde,
        DateTime? hasta,
        TipoMovimientoInventario? tipo,
        Guid? productoId,
        string? termino)
    {
        var sucursal = await ResolverSucursalAsync(sucursalId);

        var hastaFiltro = hasta.HasValue
            ? hasta.Value.Date.AddDays(1).AddMilliseconds(-1).ToUniversalTime()
            : (DateTime?)null;

        var resultado = await _inventarioService.ObtenerMovimientosAsync(
            sucursal?.Id,
            desde?.ToUniversalTime(),
            hastaFiltro,
            tipo,
            productoId,
            string.IsNullOrWhiteSpace(termino) ? null : termino);

        var productosResultado = await _productoService.ObtenerTodosAsync();
        var sucursalesResultado = await _sucursalService.ObtenerTodosAsync(true);

        string? productoNombre = null;
        if (productoId.HasValue && productosResultado.IsSuccess)
            productoNombre = productosResultado.Value!.FirstOrDefault(p => p.Id == productoId.Value)?.Nombre;

        var modelo = new InventarioMovimientosModel
        {
            SucursalId = sucursal?.Id,
            Sucursales = sucursalesResultado.IsSuccess ? sucursalesResultado.Value! : new List<SucursalDto>(),
            Desde = desde,
            Hasta = hasta,
            Tipo = tipo,
            ProductoId = productoId,
            Termino = termino,
            ProductoNombre = productoNombre,
            Productos = productosResultado.IsSuccess
                ? productosResultado.Value!.OrderBy(p => p.Nombre).ToList()
                : new List<ProductoDto>(),
            Movimientos = resultado.IsSuccess ? resultado.Value! : new List<MovimientoInventarioDto>(),
            Error = resultado.IsFailure ? resultado.Error : null,
            Mensaje = TempData["Mensaje"] as string
        };

        return View(modelo);
    }

    [HttpGet]
    [Permiso(Permisos.RegistrarMovimientosInventario)]
    public async Task<IActionResult> Registrar(Guid? productoId, Guid? sucursalId)
    {
        var sucursal = await ResolverSucursalAsync(sucursalId ?? SucursalContext.ObtenerSucursalClaim(User));
        var sucursalesResultado = await _sucursalService.ObtenerTodosAsync(true);

        List<StockProductoDto> productos = new();
        if (sucursal is not null)
        {
            var catalogo = await _inventarioService.ObtenerCatalogoStockAsync(sucursal.Id);
            if (catalogo.IsSuccess)
                productos = catalogo.Value!.OrderBy(p => p.Nombre).ToList();
        }

        var modelo = new MovimientoModel
        {
            SucursalId = sucursal?.Id ?? Guid.Empty,
            Sucursales = sucursalesResultado.IsSuccess ? sucursalesResultado.Value! : new List<SucursalDto>(),
            Productos = productos,
            Error = TempData["Error"] as string
        };

        if (productoId.HasValue)
            modelo.ProductoId = productoId.Value;

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Permiso(Permisos.RegistrarMovimientosInventario)]
    public async Task<IActionResult> Registrar(MovimientoModel modelo)
    {
        if (modelo.SucursalId == Guid.Empty)
        {
            TempData["Error"] = "Selecciona la sucursal";
            return RedirectToAction(nameof(Registrar));
        }

        if (modelo.ProductoId == Guid.Empty)
        {
            TempData["Error"] = "Selecciona un producto";
            return RedirectToAction(nameof(Registrar));
        }

        if (modelo.Cantidad <= 0)
        {
            TempData["Error"] = "La cantidad debe ser mayor a cero";
            return RedirectToAction(nameof(Registrar), new { productoId = modelo.ProductoId });
        }

        if (modelo.CostoUnitario < 0)
        {
            TempData["Error"] = "El costo unitario no puede ser negativo";
            return RedirectToAction(nameof(Registrar), new { productoId = modelo.ProductoId });
        }

        var dto = new RegistrarMovimientoDto
        {
            SucursalId = modelo.SucursalId,
            ProductoId = modelo.ProductoId,
            Tipo = modelo.Tipo,
            Cantidad = modelo.Cantidad,
            CostoUnitario = modelo.CostoUnitario,
            Referencia = string.IsNullOrWhiteSpace(modelo.Referencia) ? null : modelo.Referencia,
            Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones) ? null : modelo.Observaciones
        };

        var resultado = modelo.Tipo switch
        {
            TipoMovimientoInventario.Entrada => await _inventarioService.RegistrarEntradaAsync(dto),
            TipoMovimientoInventario.Salida => await _inventarioService.RegistrarSalidaAsync(dto),
            _ => await _inventarioService.RegistrarAjusteAsync(dto)
        };

        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = $"Movimiento registrado: {ImpuestoUI.Nombre(resultado.Value!.Tipo)} de {resultado.Value.Cantidad}";

        return RedirectToAction(nameof(Movimientos), new { sucursalId = modelo.SucursalId });
    }

    [HttpGet]
    public async Task<IActionResult> Historial(Guid productoId)
    {
        var resultado = await _inventarioService.ObtenerHistorialAsync(productoId);
        var producto = await _productoService.ObtenerPorIdAsync(productoId);

        var modelo = new HistorialModel
        {
            ProductoId = productoId,
            ProductoNombre = producto.IsSuccess ? producto.Value!.Nombre : string.Empty,
            Movimientos = resultado.IsSuccess
                ? resultado.Value!.OrderByDescending(m => m.Fecha).ToList()
                : new List<MovimientoInventarioDto>()
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

        var sucursales = await _sucursalService.ObtenerTodosAsync(true);
        return sucursales.IsSuccess ? sucursales.Value!.FirstOrDefault() : null;
    }
}