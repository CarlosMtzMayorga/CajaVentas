using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class TurnoService : ITurnoService
{
    private readonly ITurnoCajaRepository _turnoRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly ISucursalRepository _sucursalRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public TurnoService(
        ITurnoCajaRepository turnoRepository,
        IVentaRepository ventaRepository,
        ISucursalRepository sucursalRepository,
        IUsuarioRepository usuarioRepository)
    {
        _turnoRepository = turnoRepository;
        _ventaRepository = ventaRepository;
        _sucursalRepository = sucursalRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<TurnoCajaDto>> AbrirTurnoAsync(AbrirTurnoDto dto)
    {
        if (dto.FondoInicial < 0)
            return Result<TurnoCajaDto>.Failure("El fondo inicial no puede ser negativo");

        var caja = await _sucursalRepository.ObtenerCajaAsync(dto.CajaId);
        if (caja is null || !caja.Activo)
            return Result<TurnoCajaDto>.Failure("Caja no encontrada o inactiva");

        var sucursal = await _sucursalRepository.ObtenerPorIdAsync(caja.SucursalId);
        if (sucursal is null || !sucursal.Activo)
            return Result<TurnoCajaDto>.Failure("La sucursal de la caja no está activa");

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(dto.UsuarioId);
        if (usuario is null)
            return Result<TurnoCajaDto>.Failure("Usuario no encontrado");

        if (usuario.SucursalId != sucursal.Id)
            return Result<TurnoCajaDto>.Failure("No puedes abrir turno en una sucursal diferente a la asignada");

        var turnoAbierto = await _turnoRepository.ObtenerAbiertoEnCajaAsync(dto.CajaId);
        if (turnoAbierto is not null)
            return Result<TurnoCajaDto>.Failure("Ya existe un turno abierto en esta caja");

        var turno = new TurnoCaja
        {
            Id = Guid.NewGuid(),
            UsuarioId = dto.UsuarioId,
            SucursalId = sucursal.Id,
            CajaId = caja.Id,
            FechaApertura = DateTime.UtcNow,
            Estado = EstadoTurno.Abierto,
            FondoInicial = dto.FondoInicial,
            ObservacionesApertura = dto.Observaciones
        };

        await _turnoRepository.CrearAsync(turno);
        turno.Sucursal = sucursal;
        turno.Caja = caja;
        return Result<TurnoCajaDto>.Success(MapearADto(turno));
    }

    public async Task<Result<TurnoCajaDto>> CerrarTurnoAsync(Guid turnoId, CerrarTurnoDto dto)
    {
        var turno = await _turnoRepository.ObtenerPorIdAsync(turnoId);
        if (turno is null)
            return Result<TurnoCajaDto>.Failure("Turno no encontrado");

        if (turno.Estado != EstadoTurno.Abierto)
            return Result<TurnoCajaDto>.Failure("El turno ya está cerrado");

        turno.Estado = EstadoTurno.Cerrado;
        turno.FechaCierre = DateTime.UtcNow;
        turno.FondoFinal = dto.FondoFinal;
        turno.ObservacionesCierre = dto.Observaciones;

        await _turnoRepository.ActualizarAsync(turno);
        return Result<TurnoCajaDto>.Success(MapearADto(turno));
    }

    public async Task<Result<TurnoCajaDto>> ObtenerTurnoAbiertoAsync(Guid usuarioId)
    {
        var turno = await _turnoRepository.ObtenerAbiertoAsync(usuarioId);
        if (turno is null)
            return Result<TurnoCajaDto>.Failure("No hay turno abierto para este usuario");

        return Result<TurnoCajaDto>.Success(MapearADto(turno));
    }

    public async Task<Result<CorteXDto>> GenerarCorteXAsync(Guid turnoId)
    {
        var turno = await _turnoRepository.ObtenerPorIdAsync(turnoId);
        if (turno is null)
            return Result<CorteXDto>.Failure("Turno no encontrado");

        var ventas = await _ventaRepository.ObtenerPorTurnoAsync(turnoId);
        var ventasActivas = ventas.Where(v => v.Estado == EstadoVenta.Completada).ToList();

        var corte = new CorteXDto
        {
            TurnoCajaId = turno.Id,
            TotalVentas = ventasActivas.Sum(v => v.Total),
            VentasEfectivo = ventasActivas
                .Where(v => v.MetodoPago == MetodoPago.Efectivo)
                .Sum(v => v.Total),
            VentasTarjeta = ventasActivas
                .Where(v => v.MetodoPago == MetodoPago.TarjetaCredito || v.MetodoPago == MetodoPago.TarjetaDebito)
                .Sum(v => v.Total),
            CantidadVentas = ventasActivas.Count,
            FondoInicial = turno.FondoInicial,
            EnCaja = turno.FondoInicial + ventasActivas
                .Where(v => v.MetodoPago == MetodoPago.Efectivo)
                .Sum(v => v.Total),
            Fecha = DateTime.UtcNow
        };

        return Result<CorteXDto>.Success(corte);
    }

    public async Task<Result<List<TurnoCajaDto>>> ObtenerHistorialAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null)
    {
        var turnos = await _turnoRepository.ObtenerHistorialAsync(inicio, fin, sucursalId);
        return Result<List<TurnoCajaDto>>.Success(turnos.Select(MapearADto).ToList());
    }

    private static TurnoCajaDto MapearADto(TurnoCaja t) => new()
    {
        Id = t.Id,
        UsuarioId = t.UsuarioId,
        UsuarioNombre = t.Usuario?.Nombre ?? "N/A",
        SucursalId = t.SucursalId,
        SucursalNombre = t.Sucursal?.Nombre ?? "N/A",
        CajaId = t.CajaId,
        CajaNombre = t.Caja?.Nombre ?? "N/A",
        FechaApertura = t.FechaApertura,
        FechaCierre = t.FechaCierre,
        Estado = t.Estado,
        FondoInicial = t.FondoInicial,
        FondoFinal = t.FondoFinal,
        VentasEfectivo = t.VentasEfectivo,
        TotalVentas = t.TotalVentas,
        CantidadVentas = t.Ventas?.Count ?? 0
    };
}