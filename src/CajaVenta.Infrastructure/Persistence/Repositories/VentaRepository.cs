using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class VentaRepository : IVentaRepository
{
    private readonly CajaVentaDbContext _context;

    public VentaRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<Venta?> ObtenerPorIdAsync(Guid id)
        => await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.TurnoCaja)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Variante)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Venta?> ObtenerPorNumeroTicketAsync(string numeroTicket)
        => await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.NumeroTicket == numeroTicket);

    public async Task<IEnumerable<Venta>> ObtenerPorTurnoAsync(Guid turnoCajaId)
        => await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.TurnoCajaId == turnoCajaId)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();

    public async Task<Venta> CrearAsync(Venta venta)
    {
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return venta;
    }

    public async Task ActualizarAsync(Venta venta)
    {
        _context.Ventas.Update(venta);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Venta>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null, Guid? cajaId = null)
    {
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.FechaCreacion >= inicio && v.FechaCreacion <= fin)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(v => v.TurnoCaja.SucursalId == sucursalId.Value);

        if (cajaId.HasValue)
            query = query.Where(v => v.TurnoCaja.CajaId == cajaId.Value);

        return await query
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }
}