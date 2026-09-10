namespace CajaVenta.Domain.Interfaces;

public interface IRolPermisoRepository
{
    Task<IEnumerable<string>> ObtenerPermisosAsync(string rol);
    Task ReemplazarPermisosAsync(string rol, IEnumerable<string> claves);
}