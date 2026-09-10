using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Domain.Interfaces;

public interface IInventarioRepository
{
    Task<MovimientoInventario?> ObtenerPorIdAsync(Guid id);
    Task<IEnumerable<MovimientoInventario>> ObtenerPorProductoAsync(Guid productoId);
    Task<MovimientoInventario> RegistrarMovimientoAsync(MovimientoInventario movimiento);
    Task<IEnumerable<MovimientoInventario>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null);
    Task<IEnumerable<MovimientoInventario>> ObtenerFiltradoAsync(
        Guid? sucursalId,
        DateTime? desde,
        DateTime? hasta,
        TipoMovimientoInventario? tipo,
        Guid? productoId,
        string? termino);

    Task<StockInventario?> ObtenerStockAsync(Guid sucursalId, Guid productoId);
    Task<IEnumerable<StockInventario>> ObtenerCatalogoStockAsync(Guid sucursalId);
    Task ActualizarStockAsync(StockInventario stock);
    Task CrearStockAsync(StockInventario stock);
}