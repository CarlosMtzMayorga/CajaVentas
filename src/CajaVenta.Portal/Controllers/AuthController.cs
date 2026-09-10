using System.Security.Claims;
using CajaVenta.Application.Security;
using CajaVenta.Portal.Data;
using CajaVenta.Portal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Controllers;

public class AuthController : Controller
{
    private readonly PortalDbContext _context;

    public AuthController(PortalDbContext context) => _context = context;

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var usuario = await _context.UsuariosPortal.AsNoTracking()
            .FirstOrDefaultAsync(u => u.NombreUsuario == model.NombreUsuario.Trim());

        if (usuario is null || !usuario.Activo || !PasswordHasher.Verificar(model.Contrasena, usuario.ContrasenaHash))
        {
            model.MensajeError = "Usuario o contraseña incorrectos.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Role, usuario.Rol),
            new("PortalAdmin", "true")
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identidad);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}