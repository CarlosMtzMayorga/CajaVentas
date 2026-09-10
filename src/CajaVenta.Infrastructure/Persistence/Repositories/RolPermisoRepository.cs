using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class RolPermisoRepository : IRolPermisoRepository
{
    private readonly CajaVentaDbContext _context;

    public RolPermisoRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<string>> ObtenerPermisosAsync(string rol)
        => await _context.RolesPermisos
            .Where(rp => rp.Rol == rol)
            .Select(rp => rp.PermisoKey)
            .ToListAsync();

    public async Task ReemplazarPermisosAsync(string rol, IEnumerable<string> claves)
    {
        var existentes = await _context.RolesPermisos
            .Where(rp => rp.Rol == rol)
            .ToListAsync();

        _context.RolesPermisos.RemoveRange(existentes);

        var nuevas = claves
            .Distinct()
            .Select(clave => new RolPermiso { Rol = rol, PermisoKey = clave })
            .ToList();

        if (nuevas.Count > 0)
            _context.RolesPermisos.AddRange(nuevas);

        await _context.SaveChangesAsync();
    }
}