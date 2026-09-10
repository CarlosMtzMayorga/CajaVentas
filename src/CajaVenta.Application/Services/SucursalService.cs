using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class SucursalService : ISucursalService
{
    private readonly ISucursalRepository _sucursalRepository;
    private readonly ITurnoCajaRepository _turnoRepository;

    public SucursalService(ISucursalRepository sucursalRepository, ITurnoCajaRepository turnoRepository)
    {
        _sucursalRepository = sucursalRepository;
        _turnoRepository = turnoRepository;
    }

    public async Task<Result<List<SucursalDto>>> ObtenerTodosAsync(bool soloActivas = false)
    {
        var sucursales = await _sucursalRepository.ObtenerTodosAsync(soloActivas);
        return Result<List<SucursalDto>>.Success(sucursales.Select(MapearADto).ToList());
    }

    public async Task<Result<SucursalDto>> ObtenerPorIdAsync(Guid id)
    {
        var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id);
        return sucursal is null
            ? Result<SucursalDto>.Failure("Sucursal no encontrada")
            : Result<SucursalDto>.Success(MapearADto(sucursal));
    }

    public async Task<Result<SucursalDto>> CrearAsync(CrearSucursalDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<SucursalDto>.Failure("El nombre es obligatorio");

        var sucursal = new Sucursal
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Direccion = dto.Direccion?.Trim(),
            Telefono = dto.Telefono?.Trim(),
            Notas = dto.Notas?.Trim(),
            Activo = true
        };

        await _sucursalRepository.CrearAsync(sucursal);
        return Result<SucursalDto>.Success(MapearADto(sucursal));
    }

    public async Task<Result<SucursalDto>> ActualizarAsync(Guid id, ActualizarSucursalDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<SucursalDto>.Failure("El nombre es obligatorio");

        var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id);
        if (sucursal is null)
            return Result<SucursalDto>.Failure("Sucursal no encontrada");

        sucursal.Nombre = dto.Nombre.Trim();
        sucursal.Direccion = dto.Direccion?.Trim();
        sucursal.Telefono = dto.Telefono?.Trim();
        sucursal.Notas = dto.Notas?.Trim();

        await _sucursalRepository.ActualizarAsync(sucursal);
        return Result<SucursalDto>.Success(MapearADto(sucursal));
    }

    public async Task<Result<bool>> ActivarAsync(Guid id, bool activo)
    {
        var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id);
        if (sucursal is null)
            return Result<bool>.Failure("Sucursal no encontrada");

        sucursal.Activo = activo;
        await _sucursalRepository.ActualizarAsync(sucursal);
        return Result<bool>.Success(true);
    }

    public async Task<Result<List<CajaDto>>> ObtenerCajasAsync(Guid sucursalId)
    {
        var cajas = await _sucursalRepository.ObtenerCajasAsync(sucursalId);
        var lista = new List<CajaDto>();
        foreach (var c in cajas)
        {
            var turnoAbierto = await _turnoRepository.ObtenerAbiertoEnCajaAsync(c.Id);
            lista.Add(new CajaDto
            {
                Id = c.Id,
                SucursalId = c.SucursalId,
                Nombre = c.Nombre,
                Notas = c.Notas,
                Activo = c.Activo,
                TurnoAbierto = turnoAbierto is not null
            });
        }
        return Result<List<CajaDto>>.Success(lista);
    }

    public async Task<Result<CajaDto>> CrearCajaAsync(CrearCajaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<CajaDto>.Failure("El nombre es obligatorio");

        var caja = new Caja
        {
            Id = Guid.NewGuid(),
            SucursalId = dto.SucursalId,
            Nombre = dto.Nombre.Trim(),
            Notas = dto.Notas?.Trim(),
            Activo = true
        };

        await _sucursalRepository.CrearCajaAsync(caja);
        return Result<CajaDto>.Success(MapearCajaDto(caja, false));
    }

    public async Task<Result<CajaDto>> ActualizarCajaAsync(Guid cajaId, ActualizarCajaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<CajaDto>.Failure("El nombre es obligatorio");

        var caja = await _sucursalRepository.ObtenerCajaAsync(cajaId);
        if (caja is null)
            return Result<CajaDto>.Failure("Caja no encontrada");

        caja.Nombre = dto.Nombre.Trim();
        caja.Notas = dto.Notas?.Trim();

        await _sucursalRepository.ActualizarCajaAsync(caja);
        var turnoAbierto = await _turnoRepository.ObtenerAbiertoEnCajaAsync(cajaId);
        return Result<CajaDto>.Success(MapearCajaDto(caja, turnoAbierto is not null));
    }

    public async Task<Result<bool>> ActivarCajaAsync(Guid cajaId, bool activo)
    {
        var caja = await _sucursalRepository.ObtenerCajaAsync(cajaId);
        if (caja is null)
            return Result<bool>.Failure("Caja no encontrada");

        caja.Activo = activo;
        await _sucursalRepository.ActualizarCajaAsync(caja);
        return Result<bool>.Success(true);
    }

    private static SucursalDto MapearADto(Sucursal s) => new()
    {
        Id = s.Id,
        Nombre = s.Nombre,
        Direccion = s.Direccion,
        Telefono = s.Telefono,
        Notas = s.Notas,
        Activo = s.Activo,
        CantidadCajas = s.Cajas?.Count ?? 0,
        CantidadUsuarios = s.Usuarios?.Count ?? 0
    };

    private static CajaDto MapearCajaDto(Caja c, bool turnoAbierto) => new()
    {
        Id = c.Id,
        SucursalId = c.SucursalId,
        Nombre = c.Nombre,
        Notas = c.Notas,
        Activo = c.Activo,
        TurnoAbierto = turnoAbierto
    };
}