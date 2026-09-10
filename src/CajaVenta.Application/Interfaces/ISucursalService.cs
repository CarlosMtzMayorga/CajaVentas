using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;

namespace CajaVenta.Application.Interfaces;

public interface ISucursalService
{
    Task<Result<List<SucursalDto>>> ObtenerTodosAsync(bool soloActivas = false);
    Task<Result<SucursalDto>> ObtenerPorIdAsync(Guid id);
    Task<Result<SucursalDto>> CrearAsync(CrearSucursalDto dto);
    Task<Result<SucursalDto>> ActualizarAsync(Guid id, ActualizarSucursalDto dto);
    Task<Result<bool>> ActivarAsync(Guid id, bool activo);

    Task<Result<List<CajaDto>>> ObtenerCajasAsync(Guid sucursalId);
    Task<Result<CajaDto>> CrearCajaAsync(CrearCajaDto dto);
    Task<Result<CajaDto>> ActualizarCajaAsync(Guid cajaId, ActualizarCajaDto dto);
    Task<Result<bool>> ActivarCajaAsync(Guid cajaId, bool activo);
}