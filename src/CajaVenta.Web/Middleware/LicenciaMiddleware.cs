using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using CajaVenta.Web.Services;

namespace CajaVenta.Web.Middleware;

public class LicenciaMiddleware
{
    private readonly RequestDelegate _next;

    public LicenciaMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ILicenciaClienteService licencia)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var estado = await licencia.ObtenerEstadoAsync();
            if (!estado.EstaActiva && RutaRequiereLicencia(context))
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Session.Clear();
                context.Response.Redirect("/Auth/Login?licencia=inactiva");
                return;
            }
        }

        await _next(context);
    }

    private static bool RutaRequiereLicencia(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
            return false;

        if (context.Request.Path.StartsWithSegments("/Auth"))
            return false;

        var permitidoAnonimo = endpoint.Metadata
            .Any(m => m is AllowAnonymousAttribute);
        if (permitidoAnonimo)
            return false;

        var requiereAuth = endpoint.Metadata
            .Any(m => m is AuthorizeAttribute);
        return requiereAuth;
    }
}