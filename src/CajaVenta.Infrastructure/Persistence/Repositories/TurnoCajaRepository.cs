using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class TurnoCajaRepository : ITurnoCajaRepository
{
    private readonly CajaVentaDbContext _context;

    public TurnoCajaRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<TurnoCaja?> ObtenerAbiertoAsync(Guid usuarioId)
        => await _context.TurnosCaja
            .Include(t => t.Usuario)
            .Include(t => t.Sucursal)
            .Include(t => t.Caja)
            .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.Estado == Domain.Enums.EstadoTurno.Abierto);

    public async Task<TurnoCaja?> ObtenerAbiertoEnCajaAsync(Guid cajaId)
        => await _context.TurnosCaja
            .Include(t => t.Usuario)
            .Include(t => t.Sucursal)
            .Include(t => t.Caja)
            .FirstOrDefaultAsync(t => t.CajaId == cajaId && t.Estado == Domain.Enums.EstadoTurno.Abierto);

    public async Task<TurnoCaja?> ObtenerPorIdAsync(Guid id)
        => await _context.TurnosCaja
            .Include(t => t.Usuario)
            .Include(t => t.Sucursal)
            .Include(t => t.Caja)
            .Include(t => t.Ventas)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TurnoCaja> CrearAsync(TurnoCaja turno)
    {
        _context.TurnosCaja.Add(turno);
        await _context.SaveChangesAsync();
        return turno;
    }

    public async Task ActualizarAsync(TurnoCaja turno)
    {
        _context.TurnosCaja.Update(turno);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TurnoCaja>> ObtenerHistorialAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null)
    {
        var query = _context.TurnosCaja
            .Include(t => t.Usuario)
            .Include(t => t.Sucursal)
            .Include(t => t.Caja)
            .Where(t => t.FechaApertura >= inicio && t.FechaApertura <= fin)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(t => t.SucursalId == sucursalId.Value);

        return await query
            .OrderByDescending(t => t.FechaApertura)
            .ToListAsync();
    }
}