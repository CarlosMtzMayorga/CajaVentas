using CajaVenta.Domain.Entities;

namespace CajaVenta.Domain.Interfaces;

public interface ISucursalRepository
{
    Task<IEnumerable<Sucursal>> ObtenerTodosAsync(bool soloActivas = false);
    Task<Sucursal?> ObtenerPorIdAsync(Guid id);
    Task<Sucursal> CrearAsync(Sucursal sucursal);
    Task ActualizarAsync(Sucursal sucursal);

    Task<IEnumerable<Caja>> ObtenerCajasAsync(Guid sucursalId);
    Task<Caja?> ObtenerCajaAsync(Guid cajaId);
    Task<Caja> CrearCajaAsync(Caja caja);
    Task ActualizarCajaAsync(Caja caja);
}