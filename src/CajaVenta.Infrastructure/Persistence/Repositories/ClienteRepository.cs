using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly CajaVentaDbContext _context;

    public ClienteRepository(CajaVentaDbContext context)
        => _context = context;

    public async Task<Cliente?> ObtenerPorIdAsync(Guid id)
        => await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Cliente?> ObtenerPorRFCAsync(string rfc)
        => await _context.Clientes.FirstOrDefaultAsync(c => c.RFC == rfc);

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(bool soloActivos = false)
    {
        var query = _context.Clientes.AsQueryable();
        if (soloActivos)
            query = query.Where(c => c.Activo);
        return await query
            .OrderBy(c => c.NombreRazonSocial)
            .ToListAsync();
    }

    public async Task<Cliente> CrearAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task ActualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Cliente>> BuscarAsync(string termino)
    {
        var t = termino.ToLower();
        return await _context.Clientes
            .Where(c =>
                c.NombreRazonSocial.ToLower().Contains(t) ||
                c.NombreComercial != null && c.NombreComercial.ToLower().Contains(t) ||
                c.RFC.ToLower().Contains(t) ||
                c.Email != null && c.Email.ToLower().Contains(t))
            .OrderBy(c => c.NombreRazonSocial)
            .ToListAsync();
    }
}