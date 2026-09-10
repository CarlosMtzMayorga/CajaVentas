using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class SucursalRepository : ISucursalRepository
{
    private readonly CajaVentaDbContext _context;

    public SucursalRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sucursal>> ObtenerTodosAsync(bool soloActivas = false)
    {
        var query = _context.Sucursales
            .Include(s => s.Cajas)
            .Include(s => s.Usuarios)
            .AsQueryable();

        if (soloActivas)
            query = query.Where(s => s.Activo);

        return await query
            .OrderBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task<Sucursal?> ObtenerPorIdAsync(Guid id)
        => await _context.Sucursales
            .Include(s => s.Cajas)
            .Include(s => s.Usuarios)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Sucursal> CrearAsync(Sucursal sucursal)
    {
        _context.Sucursales.Add(sucursal);
        await _context.SaveChangesAsync();
        return sucursal;
    }

    public async Task ActualizarAsync(Sucursal sucursal)
    {
        _context.Sucursales.Update(sucursal);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Caja>> ObtenerCajasAsync(Guid sucursalId)
        => await _context.Cajas
            .Where(c => c.SucursalId == sucursalId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

    public async Task<Caja?> ObtenerCajaAsync(Guid cajaId)
        => await _context.Cajas
            .Include(c => c.Sucursal)
            .FirstOrDefaultAsync(c => c.Id == cajaId);

    public async Task<Caja> CrearCajaAsync(Caja caja)
    {
        _context.Cajas.Add(caja);
        await _context.SaveChangesAsync();
        return caja;
    }

    public async Task ActualizarCajaAsync(Caja caja)
    {
        _context.Cajas.Update(caja);
        await _context.SaveChangesAsync();
    }
}