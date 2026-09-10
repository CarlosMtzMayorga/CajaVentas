using CajaVenta.Domain.Common;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Domain.Entities;

public class TurnoCaja : AuditableEntity
{
    public Guid UsuarioId { get; set; }
    public Guid SucursalId { get; set; }
    public Guid CajaId { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public EstadoTurno Estado { get; set; } = EstadoTurno.Abierto;
    public decimal FondoInicial { get; set; }
    public decimal? FondoFinal { get; set; }
    public decimal VentasEfectivo { get; set; }
    public decimal TotalVentas { get; set; }
    public string? ObservacionesApertura { get; set; }
    public string? ObservacionesCierre { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Sucursal Sucursal { get; set; } = null!;
    public Caja Caja { get; set; } = null!;
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
