using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class InventarioService : IInventarioService
{
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IProductoRepository _productoRepository;

    public InventarioService(
        IInventarioRepository inventarioRepository,
        IProductoRepository productoRepository)
    {
        _inventarioRepository = inventarioRepository;
        _productoRepository = productoRepository;
    }

    public async Task<Result<MovimientoInventarioDto>> RegistrarEntradaAsync(RegistrarMovimientoDto dto)
    {
        if (dto.Cantidad <= 0)
            return Result<MovimientoInventarioDto>.Failure("La cantidad debe ser mayor a cero");

        var producto = await _productoRepository.ObtenerPorIdAsync(dto.ProductoId);
        if (producto is null)
            return Result<MovimientoInventarioDto>.Failure("Producto no encontrado");

        return await RegistrarMovimientoAsync(dto, TipoMovimientoInventario.Entrada, producto);
    }

    public async Task<Result<MovimientoInventarioDto>> RegistrarSalidaAsync(RegistrarMovimientoDto dto)
    {
        if (dto.Cantidad <= 0)
            return Result<MovimientoInventarioDto>.Failure("La cantidad debe ser mayor a cero");

        var producto = await _productoRepository.ObtenerPorIdAsync(dto.ProductoId);
        if (producto is null)
            return Result<MovimientoInventarioDto>.Failure("Producto no encontrado");

        var stock = await ObtenerStockEntidadAsync(dto.SucursalId, producto.Id);
        if (stock.StockActual < dto.Cantidad)
            return Result<MovimientoInventarioDto>.Failure(
                $"Stock insuficiente en la sucursal. Disponible: {stock.StockActual}");

        return await RegistrarMovimientoAsync(dto, TipoMovimientoInventario.Salida, producto, -dto.Cantidad);
    }

    public async Task<Result<MovimientoInventarioDto>> RegistrarAjusteAsync(RegistrarMovimientoDto dto)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(dto.ProductoId);
        if (producto is null)
            return Result<MovimientoInventarioDto>.Failure("Producto no encontrado");

        return await RegistrarMovimientoAsync(dto, TipoMovimientoInventario.Ajuste, producto, dto.Cantidad);
    }

    public async Task<Result<List<MovimientoInventarioDto>>> ObtenerHistorialAsync(Guid productoId)
    {
        var movimientos = await _inventarioRepository.ObtenerPorProductoAsync(productoId);
        return Result<List<MovimientoInventarioDto>>.Success(
            movimientos.Select(MapearADto).ToList());
    }

    public async Task<Result<List<MovimientoInventarioDto>>> ObtenerMovimientosAsync(
        Guid? sucursalId,
        DateTime? desde,
        DateTime? hasta,
        TipoMovimientoInventario? tipo,
        Guid? productoId,
        string? termino)
    {
        var movimientos = await _inventarioRepository.ObtenerFiltradoAsync(sucursalId, desde, hasta, tipo, productoId, termino);
        return Result<List<MovimientoInventarioDto>>.Success(
            movimientos.Select(MapearADto).ToList());
    }

    public async Task<Result<List<StockProductoDto>>> ObtenerCatalogoStockAsync(Guid sucursalId)
    {
        var productos = await _productoRepository.ObtenerTodosAsync();
        var stocks = (await _inventarioRepository.ObtenerCatalogoStockAsync(sucursalId))
            .ToDictionary(s => s.ProductoId, s => s.StockActual);

        var catalogo = productos
            .OrderBy(p => p.Nombre)
            .Select(p => new StockProductoDto
            {
                ProductoId = p.Id,
                CodigoBarras = p.CodigoBarras,
                Nombre = p.Nombre,
                StockActual = stocks.GetValueOrDefault(p.Id),
                StockMinimo = p.StockMinimo,
                Costo = p.Costo,
                PrecioVenta = p.PrecioVenta,
                TipoImpuesto = p.TipoImpuesto
            })
            .ToList();

        return Result<List<StockProductoDto>>.Success(catalogo);
    }

    public async Task<Result<decimal>> ObtenerStockAsync(Guid sucursalId, Guid productoId)
    {
        var stock = await _inventarioRepository.ObtenerStockAsync(sucursalId, productoId);
        return Result<decimal>.Success(stock?.StockActual ?? 0m);
    }

    private async Task<StockInventario> ObtenerStockEntidadAsync(Guid sucursalId, Guid productoId)
    {
        var stock = await _inventarioRepository.ObtenerStockAsync(sucursalId, productoId);
        if (stock is not null)
            return stock;

        stock = new StockInventario
        {
            ProductoId = productoId,
            SucursalId = sucursalId,
            StockActual = 0
        };
        await _inventarioRepository.CrearStockAsync(stock);
        return stock;
    }

    private async Task<Result<MovimientoInventarioDto>> RegistrarMovimientoAsync(
        RegistrarMovimientoDto dto,
        TipoMovimientoInventario tipo,
        Producto producto,
        decimal? cantidadAjustada = null)
    {
        var stock = await ObtenerStockEntidadAsync(dto.SucursalId, producto.Id);

        var nuevoStock = stock.StockActual + (cantidadAjustada ?? dto.Cantidad);
        if (nuevoStock < 0)
            return Result<MovimientoInventarioDto>.Failure($"El movimiento dejaría el stock en negativo ({nuevoStock})");

        stock.StockActual = nuevoStock;
        await _inventarioRepository.ActualizarStockAsync(stock);

        var movimiento = new MovimientoInventario
        {
            Id = Guid.NewGuid(),
            ProductoId = producto.Id,
            SucursalId = dto.SucursalId,
            Tipo = tipo,
            Cantidad = cantidadAjustada ?? dto.Cantidad,
            CostoUnitario = dto.CostoUnitario,
            Referencia = dto.Referencia,
            Observaciones = dto.Observaciones,
            Fecha = DateTime.UtcNow
        };
        await _inventarioRepository.RegistrarMovimientoAsync(movimiento);

        return Result<MovimientoInventarioDto>.Success(MapearADto(movimiento));
    }

    private static MovimientoInventarioDto MapearADto(MovimientoInventario m) => new()
    {
        Id = m.Id,
        ProductoNombre = m.Producto?.Nombre ?? "N/A",
        ProductoCodigoBarras = m.Producto?.CodigoBarras ?? string.Empty,
        SucursalId = m.SucursalId,
        SucursalNombre = m.Sucursal?.Nombre ?? "N/A",
        Tipo = m.Tipo,
        Cantidad = m.Cantidad,
        CostoUnitario = m.CostoUnitario,
        Referencia = m.Referencia,
        Observaciones = m.Observaciones,
        Fecha = m.Fecha
    };
}