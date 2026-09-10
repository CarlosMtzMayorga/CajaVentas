using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly CajaVentaDbContext _context;

    public ProductoRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<Producto?> ObtenerPorIdAsync(Guid id)
        => await _context.Productos
            .Include(p => p.Variantes.Where(v => v.Activo))
            .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

    public async Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras)
        => await _context.Productos
            .Include(p => p.Variantes.Where(v => v.Activo))
            .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras && p.Activo);

    public async Task<IEnumerable<Producto>> ObtenerTodosAsync()
        => await _context.Productos
            .Include(p => p.Variantes.Where(v => v.Activo))
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<Producto>> BuscarAsync(string termino)
        => await _context.Productos
            .Include(p => p.Variantes.Where(v => v.Activo))
            .Where(p => p.Activo &&
                (p.Nombre.Contains(termino) ||
                 p.CodigoBarras.Contains(termino) ||
                 (p.Descripcion != null && p.Descripcion.Contains(termino))))
            .OrderBy(p => p.Nombre)
            .ToListAsync();

    public async Task<Producto> CrearAsync(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    public async Task ActualizarAsync(Producto producto)
    {
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, Guid? excludeId = null)
        => await _context.Productos
            .AnyAsync(p => p.CodigoBarras == codigoBarras &&
                p.Activo &&
                (!excludeId.HasValue || p.Id != excludeId.Value));
}
