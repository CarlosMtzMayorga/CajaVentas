using CajaVenta.Application.DTOs;

namespace CajaVenta.Web.Models;

public class TurnosIndexModel
{
    public TurnoCajaDto? TurnoAbierto { get; set; }
    public List<TurnoCajaDto> Historial { get; set; } = new();
    public List<CajaDto> Cajas { get; set; } = new();
    public string? SucursalNombre { get; set; }
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}