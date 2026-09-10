namespace CajaVenta.Application.Interfaces;

public interface IPermisoService
{
    Task<bool> TienePermisoAsync(string rol, string clave);
    Task<IEnumerable<string>> ObtenerPermisosRolAsync(string rol);
    Task<bool> ActualizarPermisosAsync(string rol, IEnumerable<string> claves);
}