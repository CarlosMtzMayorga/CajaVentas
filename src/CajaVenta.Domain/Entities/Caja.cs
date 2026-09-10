using CajaVenta.Domain.Common;

namespace CajaVenta.Domain.Entities;

public class Caja : AuditableEntity
{
    public Guid SucursalId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Notas { get; set; }

    public Sucursal Sucursal { get; set; } = null!;
    public ICollection<TurnoCaja> Turnos { get; set; } = new List<TurnoCaja>();
}