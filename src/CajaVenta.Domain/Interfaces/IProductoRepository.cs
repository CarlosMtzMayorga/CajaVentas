using CajaVenta.Domain.Entities;

namespace CajaVenta.Domain.Interfaces;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorCodigoBarrasAsync(string codigoBarras);
    Task<Producto?> ObtenerPorIdAsync(Guid id);
    Task<IEnumerable<Producto>> ObtenerTodosAsync();
    Task<IEnumerable<Producto>> BuscarAsync(string termino);
    Task<Producto> CrearAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
    Task<bool> ExisteCodigoBarrasAsync(string codigoBarras, Guid? excludeId = null);
}
