using CajaVenta.Domain.Common;

namespace CajaVenta.Domain.Entities;

public class Cliente : AuditableEntity
{
    public string NombreRazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string RFC { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? CodigoPostal { get; set; }
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? ConstanciaRuta { get; set; }
    public string? ConstanciaOriginalNombre { get; set; }
    public string? Notas { get; set; }

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}