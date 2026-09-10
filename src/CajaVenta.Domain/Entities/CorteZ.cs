using CajaVenta.Domain.Common;

namespace CajaVenta.Domain.Entities;

public class CorteZ : AuditableEntity
{
    public int Numero { get; set; }
    public Guid TurnoCajaId { get; set; }
    public Guid SucursalId { get; set; }
    public Guid CajaId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime FechaCierre { get; set; }
    public decimal FondoInicial { get; set; }
    public decimal? FondoFinal { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal TotalVentas { get; set; }
    public int CantidadVentas { get; set; }
    public int CantidadCanceladas { get; set; }
    public decimal VentasEfectivo { get; set; }
    public decimal VentasTarjeta { get; set; }
    public decimal VentasOtros { get; set; }

    public TurnoCaja TurnoCaja { get; set; } = null!;
    public Sucursal Sucursal { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}