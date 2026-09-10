using CajaVenta.Domain.Entities;

namespace CajaVenta.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id);
    Task<Cliente?> ObtenerPorRFCAsync(string rfc);
    Task<IEnumerable<Cliente>> ObtenerTodosAsync(bool soloActivos = false);
    Task<Cliente> CrearAsync(Cliente cliente);
    Task ActualizarAsync(Cliente cliente);
    Task<IEnumerable<Cliente>> BuscarAsync(string termino);
}