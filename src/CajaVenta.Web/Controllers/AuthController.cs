using System.Security.Claims;
using CajaVenta.Infrastructure.Persistence;
using CajaVenta.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Web.Controllers;

public class AuthController : Controller
{
    private readonly CajaVentaDbContext _context;

    public AuthController(CajaVentaDbContext context)
        => _context = context;

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Pos");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string nombreUsuario, string contrasena, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            ModelState.AddModelError(string.Empty, "Ingresa usuario y contraseña");
            return View();
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == nombreUsuario.Trim().ToLower());

        if (usuario is null || !PasswordHasher.Verificar(contrasena, usuario.ContrasenaHash))
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos");
            return View();
        }

        if (!usuario.Activo)
        {
            ModelState.AddModelError(string.Empty, "El usuario está inactivo");
            return View();
        }

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.Rol ?? "Cajero"),
            new("Nombre", usuario.Nombre),
            new("UsuarioId", usuario.Id.ToString())
        };

        if (usuario.SucursalId.HasValue)
            claims.Add(new Claim("SucursalId", usuario.SucursalId.Value.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(10)
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Pos");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}