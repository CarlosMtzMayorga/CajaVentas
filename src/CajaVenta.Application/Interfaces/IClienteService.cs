using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;

namespace CajaVenta.Application.Interfaces;

public interface IClienteService
{
    Task<Result<ClienteDto>> ObtenerPorIdAsync(Guid id);
    Task<Result<List<ClienteDto>>> ObtenerTodosAsync(bool soloActivos = false);
    Task<Result<List<ClienteDto>>> BuscarAsync(string termino);
    Task<Result<ClienteDto>> CrearAsync(CrearClienteDto dto);
    Task<Result<ClienteDto>> ActualizarAsync(Guid id, ActualizarClienteDto dto);
    Task<Result<bool>> ActivarAsync(Guid id, bool activo);
}