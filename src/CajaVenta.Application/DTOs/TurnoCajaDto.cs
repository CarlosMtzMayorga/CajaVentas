using CajaVenta.Domain.Enums;

namespace CajaVenta.Application.DTOs;

public record TurnoCajaDto
{
    public Guid Id { get; init; }
    public Guid UsuarioId { get; init; }
    public string UsuarioNombre { get; init; } = string.Empty;
    public Guid SucursalId { get; init; }
    public string SucursalNombre { get; init; } = string.Empty;
    public Guid CajaId { get; init; }
    public string CajaNombre { get; init; } = string.Empty;
    public DateTime FechaApertura { get; init; }
    public DateTime? FechaCierre { get; init; }
    public EstadoTurno Estado { get; init; }
    public decimal FondoInicial { get; init; }
    public decimal? FondoFinal { get; init; }
    public decimal VentasEfectivo { get; init; }
    public decimal TotalVentas { get; init; }
    public int CantidadVentas { get; init; }
}

public record AbrirTurnoDto
{
    public Guid UsuarioId { get; init; }
    public Guid CajaId { get; init; }
    public decimal FondoInicial { get; init; }
    public string? Observaciones { get; init; }
}

public record CerrarTurnoDto
{
    public decimal FondoFinal { get; init; }
    public string? Observaciones { get; init; }
}

public record CorteXDto
{
    public Guid TurnoCajaId { get; init; }
    public decimal TotalVentas { get; init; }
    public decimal VentasEfectivo { get; init; }
    public decimal VentasTarjeta { get; init; }
    public int CantidadVentas { get; init; }
    public decimal FondoInicial { get; init; }
    public decimal EnCaja { get; init; }
    public DateTime Fecha { get; init; }
}

public record CorteZDto
{
    public Guid Id { get; init; }
    public int Numero { get; init; }
    public Guid TurnoCajaId { get; init; }
    public Guid SucursalId { get; init; }
    public string SucursalNombre { get; init; } = string.Empty;
    public Guid CajaId { get; init; }
    public string CajaNombre { get; init; } = string.Empty;
    public Guid UsuarioId { get; init; }
    public string UsuarioNombre { get; init; } = string.Empty;
    public DateTime FechaApertura { get; init; }
    public DateTime FechaCierre { get; init; }
    public decimal FondoInicial { get; init; }
    public decimal? FondoFinal { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Impuestos { get; init; }
    public decimal TotalVentas { get; init; }
    public int CantidadVentas { get; init; }
    public int CantidadCanceladas { get; init; }
    public decimal VentasEfectivo { get; init; }
    public decimal VentasTarjeta { get; init; }
    public decimal VentasOtros { get; init; }
}
