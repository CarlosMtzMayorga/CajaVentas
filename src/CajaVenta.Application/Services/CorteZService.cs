using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class CorteZService : ICorteZService
{
    private readonly ICorteZRepository _corteRepository;
    private readonly ITurnoCajaRepository _turnoRepository;
    private readonly IVentaRepository _ventaRepository;

    public CorteZService(
        ICorteZRepository corteRepository,
        ITurnoCajaRepository turnoRepository,
        IVentaRepository ventaRepository)
    {
        _corteRepository = corteRepository;
        _turnoRepository = turnoRepository;
        _ventaRepository = ventaRepository;
    }

    public async Task<Result<CorteZDto>> GenerarCorteZAsync(Guid turnoCajaId)
    {
        var existente = await _corteRepository.ObtenerPorTurnoAsync(turnoCajaId);
        if (existente is not null)
            return Result<CorteZDto>.Success(MapearADto(existente)!);

        var turno = await _turnoRepository.ObtenerPorIdAsync(turnoCajaId);
        if (turno is null)
            return Result<CorteZDto>.Failure("Turno no encontrado");

        if (turno.Estado != EstadoTurno.Cerrado)
            return Result<CorteZDto>.Failure("El corte Z solo puede generarse sobre un turno cerrado");

        var ventas = await _ventaRepository.ObtenerPorTurnoAsync(turnoCajaId);
        var completadas = ventas.Where(v => v.Estado == EstadoVenta.Completada).ToList();
        var canceladas = ventas.Where(v => v.Estado == EstadoVenta.Cancelada).ToList();

        var nextNumero = await _corteRepository.ObtenerSiguienteNumeroAsync(turno.CajaId);

        var corte = new CorteZ
        {
            Id = Guid.NewGuid(),
            Numero = nextNumero,
            TurnoCajaId = turno.Id,
            SucursalId = turno.SucursalId,
            CajaId = turno.CajaId,
            UsuarioId = turno.UsuarioId,
            FechaApertura = turno.FechaApertura,
            FechaCierre = turno.FechaCierre ?? DateTime.UtcNow,
            FondoInicial = turno.FondoInicial,
            FondoFinal = turno.FondoFinal,
            Subtotal = completadas.Sum(v => v.Subtotal),
            Impuestos = completadas.Sum(v => v.Impuestos),
            TotalVentas = completadas.Sum(v => v.Total),
            CantidadVentas = completadas.Count,
            CantidadCanceladas = canceladas.Count,
            VentasEfectivo = completadas.Where(v => v.MetodoPago == MetodoPago.Efectivo).Sum(v => v.Total),
            VentasTarjeta = completadas
                .Where(v => v.MetodoPago == MetodoPago.TarjetaCredito || v.MetodoPago == MetodoPago.TarjetaDebito)
                .Sum(v => v.Total),
            VentasOtros = completadas
                .Where(v => v.MetodoPago is MetodoPago.Transferencia or MetodoPago.QR)
                .Sum(v => v.Total)
        };

        await _corteRepository.CrearAsync(corte);
        return Result<CorteZDto>.Success(MapearADto(corte)!);
    }

    public async Task<Result<CorteZDto>> ObtenerPorTurnoAsync(Guid turnoCajaId)
    {
        var corte = await _corteRepository.ObtenerPorTurnoAsync(turnoCajaId);
        if (corte is null)
            return Result<CorteZDto>.Failure("No hay corte Z para este turno");

        return Result<CorteZDto>.Success(MapearADto(corte)!);
    }

    public async Task<Result<CorteZDto>> ObtenerPorIdAsync(Guid id)
    {
        var corte = await _corteRepository.ObtenerPorIdAsync(id);
        if (corte is null)
            return Result<CorteZDto>.Failure("Corte Z no encontrado");

        return Result<CorteZDto>.Success(MapearADto(corte)!);
    }

    public async Task<Result<List<CorteZDto>>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null, Guid? cajaId = null)
    {
        var cortes = await _corteRepository.ObtenerPorRangoFechasAsync(inicio, fin, sucursalId, cajaId);
        return Result<List<CorteZDto>>.Success(
            cortes.Select(c => MapearADto(c)!).OrderByDescending(c => c.FechaCierre).ToList());
    }

    private static CorteZDto? MapearADto(CorteZ c) => new()
    {
        Id = c.Id,
        Numero = c.Numero,
        TurnoCajaId = c.TurnoCajaId,
        SucursalId = c.SucursalId,
        SucursalNombre = c.Sucursal?.Nombre ?? "N/A",
        CajaId = c.CajaId,
        CajaNombre = c.Caja?.Nombre ?? "N/A",
        UsuarioId = c.UsuarioId,
        UsuarioNombre = c.Usuario?.Nombre ?? "N/A",
        FechaApertura = c.FechaApertura,
        FechaCierre = c.FechaCierre,
        FondoInicial = c.FondoInicial,
        FondoFinal = c.FondoFinal,
        Subtotal = c.Subtotal,
        Impuestos = c.Impuestos,
        TotalVentas = c.TotalVentas,
        CantidadVentas = c.CantidadVentas,
        CantidadCanceladas = c.CantidadCanceladas,
        VentasEfectivo = c.VentasEfectivo,
        VentasTarjeta = c.VentasTarjeta,
        VentasOtros = c.VentasOtros
    };
}