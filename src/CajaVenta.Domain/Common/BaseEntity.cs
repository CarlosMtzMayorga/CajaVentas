namespace CajaVenta.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public bool Activo { get; set; } = true;
}

public abstract class AuditableEntity : BaseEntity
{
    public string? CreadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}
