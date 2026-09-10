using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;

namespace CajaVenta.Application.Interfaces;

public interface IProductoService
{
    Task<Result<ProductoDto>> ObtenerPorIdAsync(Guid id);
    Task<Result<ProductoDto>> ObtenerPorCodigoBarrasAsync(string codigoBarras);
    Task<Result<List<ProductoDto>>> ObtenerTodosAsync();
    Task<Result<List<ProductoDto>>> BuscarAsync(string termino);
    Task<Result<ProductoDto>> CrearAsync(CrearProductoDto dto);
    Task<Result<ProductoDto>> ActualizarAsync(Guid id, ActualizarProductoDto dto);
    Task<Result<bool>> EliminarAsync(Guid id);
}