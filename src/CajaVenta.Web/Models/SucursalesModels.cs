using CajaVenta.Application.DTOs;

namespace CajaVenta.Web.Models;

public class SucursalesIndexModel
{
    public List<SucursalDto> Sucursales { get; set; } = new();
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public class EditarSucursalModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Notas { get; set; }
}

public class CajasIndexModel
{
    public SucursalDto Sucursal { get; set; } = new();
    public List<CajaDto> Cajas { get; set; } = new();
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public class EditarCajaModel
{
    public Guid Id { get; set; }
    public Guid SucursalId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Notas { get; set; }
}