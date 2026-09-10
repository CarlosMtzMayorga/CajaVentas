using System.Security.Claims;

namespace CajaVenta.Web;

public static class SucursalContext
{
    public static Guid? ObtenerSucursalClaim(ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue("SucursalId"), out var id) ? id : null;
}