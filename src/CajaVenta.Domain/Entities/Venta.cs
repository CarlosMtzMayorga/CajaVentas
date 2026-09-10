using CajaVenta.Domain.Common;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Domain.Entities;

public class Venta : AuditableEntity
{
    public string NumeroTicket { get; set; } = string.Empty;
    public Guid TurnoCajaId { get; set; }
    public EstadoVenta Estado { get; set; } = EstadoVenta.Completada;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
    public decimal MontoRecibido { get; set; }
    public decimal Cambio { get; set; }
    public string? Observaciones { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public TurnoCaja TurnoCaja { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
