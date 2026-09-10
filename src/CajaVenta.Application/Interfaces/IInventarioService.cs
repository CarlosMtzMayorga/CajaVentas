using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Application.Interfaces;

public interface IInventarioService
{
    Task<Result<MovimientoInventarioDto>> RegistrarEntradaAsync(RegistrarMovimientoDto dto);
    Task<Result<MovimientoInventarioDto>> RegistrarSalidaAsync(RegistrarMovimientoDto dto);
    Task<Result<MovimientoInventarioDto>> RegistrarAjusteAsync(RegistrarMovimientoDto dto);
    Task<Result<List<MovimientoInventarioDto>>> ObtenerHistorialAsync(Guid productoId);
    Task<Result<List<MovimientoInventarioDto>>> ObtenerMovimientosAsync(
        Guid? sucursalId,
        DateTime? desde,
        DateTime? hasta,
        TipoMovimientoInventario? tipo,
        Guid? productoId,
        string? termino);
    Task<Result<List<StockProductoDto>>> ObtenerCatalogoStockAsync(Guid sucursalId);
    Task<Result<decimal>> ObtenerStockAsync(Guid sucursalId, Guid productoId);
}