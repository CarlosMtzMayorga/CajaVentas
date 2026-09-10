using CajaVenta.Domain.Common;

namespace CajaVenta.Domain.Entities;

public class Sucursal : AuditableEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Notas { get; set; }

    public ICollection<Caja> Cajas { get; set; } = new List<Caja>();
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<TurnoCaja> Turnos { get; set; } = new List<TurnoCaja>();
    public ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
    public ICollection<StockInventario> StocksInventario { get; set; } = new List<StockInventario>();
}