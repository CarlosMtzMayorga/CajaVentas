using CajaVenta.Domain.Enums;

namespace CajaVenta.Application.DTOs;

public record VentaDto
{
    public Guid Id { get; init; }
    public string NumeroTicket { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
    public EstadoVenta Estado { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Impuestos { get; init; }
    public decimal Total { get; init; }
    public MetodoPago MetodoPago { get; init; }
    public decimal MontoRecibido { get; init; }
    public decimal Cambio { get; init; }
    public Guid? ClienteId { get; init; }
    public string? ClienteNombre { get; init; }
    public string? ClienteRfc { get; init; }
    public List<DetalleVentaDto> Detalles { get; init; } = new();
}

public record DetalleVentaDto
{
    public Guid Id { get; init; }
    public string ProductoNombre { get; init; } = string.Empty;
    public string ProductoCodigoBarras { get; init; } = string.Empty;
    public decimal Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Impuesto { get; init; }
    public decimal Total { get; init; }
}

public record CrearVentaDto
{
    public Guid TurnoCajaId { get; init; }
    public Guid? ClienteId { get; init; }
    public MetodoPago MetodoPago { get; init; }
    public decimal MontoRecibido { get; init; }
    public string? Observaciones { get; init; }
    public List<CrearDetalleVentaDto> Detalles { get; init; } = new();
}

public record CrearDetalleVentaDto
{
    public Guid ProductoId { get; init; }
    public Guid? VarianteId { get; init; }
    public decimal Cantidad { get; init; }
}
