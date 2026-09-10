using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;

namespace CajaVenta.Portal.Models;

public class LoginViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string? MensajeError { get; set; }
}

public class DashboardViewModel
{
    public int TotalSuscriptores { get; set; }
    public int Activos { get; set; }
    public int Suspendidos { get; set; }
    public int Cancelados { get; set; }
    public int EnPrueba { get; set; }
    public int SuscripcionesActivas { get; set; }
    public int SuscripcionesVencidas { get; set; }
    public decimal IngresosMes { get; set; }
    public decimal IngresosTotales { get; set; }
    public List<Suscripcion> PorVencer { get; set; } = new();
    public List<Pago> UltimosPagos { get; set; } = new();
}

public class SuscriptorViewModel
{
    public Guid Id { get; set; }
    public string NombreComercial { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? Rfc { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string UrlAcceso { get; set; } = string.Empty;
    public List<Suscripcion> Suscripciones { get; set; } = new();
    public List<Pago> Pagos { get; set; } = new();
    public EstateSelect Estado { get; set; } = new();
}

public class EstateSelect
{
    public string Valor { get; set; } = string.Empty;
    public List<EstadoSuscriptor> Opciones => Enum.GetValues<EstadoSuscriptor>().ToList();
}

public class PlanViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public bool Activo { get; set; }
}

public class SuscripcionViewModel
{
    public Guid Id { get; set; }
    public Guid SuscriptorId { get; set; }
    public Guid PlanId { get; set; }
    public decimal PrecioMensual { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public EstadoSuscripcion Estado { get; set; }
    public int Meses { get; set; } = 1;
    public List<Suscriptor> Suscriptores { get; set; } = new();
    public List<Plan> Planes { get; set; } = new();
}

public class PagoViewModel
{
    public Guid SuscriptorId { get; set; }
    public Guid SuscripcionId { get; set; }
    public decimal Monto { get; set; }
    public MetodoPagoPasarela Metodo { get; set; }
    public string? Notas { get; set; }
    public List<Suscriptor> Suscriptores { get; set; } = new();
    public List<Suscripcion> Suscripciones { get; set; } = new();
}