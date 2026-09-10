using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class CorteZRepository : ICorteZRepository
{
    private readonly CajaVentaDbContext _context;

    public CorteZRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<CorteZ?> ObtenerPorTurnoAsync(Guid turnoCajaId)
        => await _context.CortesZ
            .Include(c => c.Sucursal)
            .Include(c => c.Caja)
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.TurnoCajaId == turnoCajaId);

    public async Task<CorteZ?> ObtenerPorIdAsync(Guid id)
        => await _context.CortesZ
            .Include(c => c.Sucursal)
            .Include(c => c.Caja)
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<CorteZ> CrearAsync(CorteZ corte)
    {
        _context.CortesZ.Add(corte);
        await _context.SaveChangesAsync();
        return corte;
    }

    public async Task<int> ObtenerSiguienteNumeroAsync(Guid cajaId)
    {
        var maximo = await _context.CortesZ
            .Where(c => c.CajaId == cajaId)
            .MaxAsync(c => (int?)c.Numero);

        return (maximo ?? 0) + 1;
    }

    public async Task<IEnumerable<CorteZ>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null, Guid? cajaId = null)
    {
        var query = _context.CortesZ
            .Include(c => c.Sucursal)
            .Include(c => c.Caja)
            .Include(c => c.Usuario)
            .Where(c => c.FechaCierre >= inicio && c.FechaCierre <= fin)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(c => c.SucursalId == sucursalId.Value);

        if (cajaId.HasValue)
            query = query.Where(c => c.CajaId == cajaId.Value);

        return await query
            .OrderByDescending(c => c.FechaCierre)
            .ToListAsync();
    }
}