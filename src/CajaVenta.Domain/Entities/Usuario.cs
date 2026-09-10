using CajaVenta.Domain.Common;

namespace CajaVenta.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Rol { get; set; }
    public Guid? SucursalId { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    public Sucursal? Sucursal { get; set; }
    public ICollection<TurnoCaja> Turnos { get; set; } = new List<TurnoCaja>();
}
