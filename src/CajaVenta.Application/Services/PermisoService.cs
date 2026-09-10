using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Common;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class PermisoService : IPermisoService
{
    private const string RolAdmin = "Admin";
    private readonly IRolPermisoRepository _rolPermisoRepository;

    public PermisoService(IRolPermisoRepository rolPermisoRepository)
    {
        _rolPermisoRepository = rolPermisoRepository;
    }

    public async Task<bool> TienePermisoAsync(string rol, string clave)
    {
        if (string.IsNullOrWhiteSpace(rol) || string.IsNullOrWhiteSpace(clave))
            return false;

        if (rol.Equals(RolAdmin, StringComparison.OrdinalIgnoreCase))
            return true;

        var permisos = await ObtenerPermisosRolAsync(rol);
        return permisos.Contains(clave, StringComparer.OrdinalIgnoreCase);
    }

    public Task<IEnumerable<string>> ObtenerPermisosRolAsync(string rol)
        => _rolPermisoRepository.ObtenerPermisosAsync(rol);

    public async Task<bool> ActualizarPermisosAsync(string rol, IEnumerable<string> claves)
    {
        if (string.IsNullOrWhiteSpace(rol))
            return false;

        if (rol.Equals(RolAdmin, StringComparison.OrdinalIgnoreCase))
            return true;

        var validas = claves
            .Distinct()
            .Where(clave => Permisos.Todos.Contains(clave))
            .ToList();

        await _rolPermisoRepository.ReemplazarPermisosAsync(rol, validas);
        return true;
    }
}