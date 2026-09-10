namespace CajaVenta.Web.Models;

public class PermisologiaModel
{
    public List<RolPermisosRow> Roles { get; set; } = new();
}

public class RolPermisosRow
{
    public string Rol { get; set; } = string.Empty;
    public HashSet<string> PermisosActivos { get; set; } = new();
}