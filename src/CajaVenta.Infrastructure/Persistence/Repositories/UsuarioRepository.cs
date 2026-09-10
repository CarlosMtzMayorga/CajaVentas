using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly CajaVentaDbContext _context;

    public UsuarioRepository(CajaVentaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        => await _context.Usuarios
            .Include(u => u.Sucursal)
            .OrderBy(u => u.Nombre)
            .ToListAsync();

    public async Task<Usuario?> ObtenerPorIdAsync(Guid id)
        => await _context.Usuarios
            .Include(u => u.Sucursal)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        => await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}