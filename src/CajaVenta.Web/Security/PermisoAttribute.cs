using CajaVenta.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CajaVenta.Web.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class PermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _clave;

    public PermisoAttribute(string clave)
    {
        _clave = clave;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var rol = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (rol == "Admin")
            return;

        var servicio = context.HttpContext.RequestServices.GetRequiredService<IPermisoService>();
        var autorizado = await servicio.TienePermisoAsync(rol ?? string.Empty, _clave);

        if (!autorizado)
            context.Result = new RedirectToActionResult("AccessDenied", "Home", new { recurso = _clave });
    }
}