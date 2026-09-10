using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class InventarioRepository : IInventarioRepository
{
    private readonly CajaVentaDbContext _context;

    public InventarioRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<MovimientoInventario?> ObtenerPorIdAsync(Guid id)
        => await _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.Sucursal)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<MovimientoInventario>> ObtenerPorProductoAsync(Guid productoId)
        => await _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.Sucursal)
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

    public async Task<MovimientoInventario> RegistrarMovimientoAsync(MovimientoInventario movimiento)
    {
        _context.MovimientosInventario.Add(movimiento);
        await _context.SaveChangesAsync();
        return movimiento;
    }

    public async Task<IEnumerable<MovimientoInventario>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null)
    {
        var query = _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.Sucursal)
            .Where(m => m.Fecha >= inicio && m.Fecha <= fin)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(m => m.SucursalId == sucursalId.Value);

        return await query
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();
    }

    public async Task<IEnumerable<MovimientoInventario>> ObtenerFiltradoAsync(
        Guid? sucursalId,
        DateTime? desde,
        DateTime? hasta,
        TipoMovimientoInventario? tipo,
        Guid? productoId,
        string? termino)
    {
        var query = _context.MovimientosInventario
            .Include(m => m.Producto)
            .Include(m => m.Sucursal)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(m => m.SucursalId == sucursalId.Value);
        if (desde.HasValue)
            query = query.Where(m => m.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(m => m.Fecha <= hasta.Value);
        if (tipo.HasValue)
            query = query.Where(m => m.Tipo == tipo.Value);
        if (productoId.HasValue)
            query = query.Where(m => m.ProductoId == productoId.Value);
        if (!string.IsNullOrWhiteSpace(termino))
        {
            var t = termino.Trim().ToLower();
            query = query.Where(m =>
                m.Producto.Nombre.ToLower().Contains(t) ||
                m.Producto.CodigoBarras.ToLower().Contains(t) ||
                (m.Referencia != null && m.Referencia.ToLower().Contains(t)));
        }

        return await query
            .OrderByDescending(m => m.Fecha)
            .ThenBy(m => m.Producto.Nombre)
            .ToListAsync();
    }

    public async Task<StockInventario?> ObtenerStockAsync(Guid sucursalId, Guid productoId)
        => await _context.StocksInventario
            .FirstOrDefaultAsync(s => s.SucursalId == sucursalId && s.ProductoId == productoId);

    public async Task<IEnumerable<StockInventario>> ObtenerCatalogoStockAsync(Guid sucursalId)
        => await _context.StocksInventario
            .Include(s => s.Producto)
            .Where(s => s.SucursalId == sucursalId)
            .ToListAsync();

    public async Task ActualizarStockAsync(StockInventario stock)
    {
        _context.StocksInventario.Update(stock);
        await _context.SaveChangesAsync();
    }

    public async Task CrearStockAsync(StockInventario stock)
    {
        _context.StocksInventario.Add(stock);
        await _context.SaveChangesAsync();
    }
}