namespace CajaVenta.Portal.Entities;

public class Suscriptor
{
    public Guid Id { get; set; }
    public string NombreComercial { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Rfc { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string UrlAcceso { get; set; } = string.Empty;
    public string RutaBaseDatos { get; set; } = string.Empty;
    public Enums.EstadoSuscriptor Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
}

public class Plan
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
}

public class Suscripcion
{
    public Guid Id { get; set; }
    public Guid SuscriptorId { get; set; }
    public Guid PlanId { get; set; }
    public Enums.EstadoSuscripcion Estado { get; set; }
    public decimal PrecioMensual { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Suscriptor Suscriptor { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}

public class Pago
{
    public Guid Id { get; set; }
    public Guid SuscriptorId { get; set; }
    public Guid SuscripcionId { get; set; }
    public decimal Monto { get; set; }
    public Enums.MetodoPagoPasarela Metodo { get; set; }
    public Enums.EstadoPago Estado { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? Notas { get; set; }
    public DateTime Fecha { get; set; }

    public Suscriptor Suscriptor { get; set; } = null!;
    public Suscripcion Suscripcion { get; set; } = null!;
}

public class UsuarioPortal
{
    public Guid Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = "Admin";
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
}