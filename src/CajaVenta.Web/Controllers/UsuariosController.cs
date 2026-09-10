using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly ISucursalService _sucursalService;

    public UsuariosController(IUsuarioService usuarioService, ISucursalService sucursalService)
    {
        _usuarioService = usuarioService;
        _sucursalService = sucursalService;
    }

    public async Task<IActionResult> Index()
    {
        var resultado = await _usuarioService.ObtenerTodosAsync();
        var modelo = new UsuariosIndexModel
        {
            Usuarios = resultado.IsSuccess ? resultado.Value! : new List<UsuarioDto>(),
            Error = resultado.IsFailure ? resultado.Error : null,
            Mensaje = TempData["Mensaje"] as string
        };
        return View(modelo);
    }

    public async Task<IActionResult> Crear()
    {
        return View(new CrearUsuarioModel
        {
            Sucursales = await ObtenerSucursalesAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioModel modelo)
    {
        modelo.Sucursales = await ObtenerSucursalesAsync();

        var resultado = await _usuarioService.CrearAsync(new CrearUsuarioDto
        {
            Nombre = modelo.Nombre,
            NombreUsuario = modelo.NombreUsuario,
            Contrasena = modelo.Contrasena,
            Email = modelo.Email,
            Rol = modelo.Rol,
            SucursalId = modelo.SucursalId
        });

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Mensaje"] = $"Usuario creado: {resultado.Value!.NombreUsuario}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(Guid id)
    {
        var resultado = await _usuarioService.ObtenerPorIdAsync(id);
        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        var u = resultado.Value!;
        return View(new EditarUsuarioModel
        {
            Id = u.Id,
            Nombre = u.Nombre,
            NombreUsuario = u.NombreUsuario,
            Email = u.Email,
            Rol = u.Rol,
            SucursalId = u.SucursalId,
            Sucursales = await ObtenerSucursalesAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, EditarUsuarioModel modelo)
    {
        modelo.Sucursales = await ObtenerSucursalesAsync();

        var resultado = await _usuarioService.ActualizarAsync(id, new ActualizarUsuarioDto
        {
            Nombre = modelo.Nombre,
            Email = modelo.Email,
            Rol = modelo.Rol,
            SucursalId = modelo.SucursalId
        });

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Mensaje"] = "Usuario actualizado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarActivo(Guid id, bool activo)
    {
        var resultado = await _usuarioService.ActivarAsync(id, activo);
        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = activo ? "Usuario activado" : "Usuario desactivado";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReiniciarContrasena(Guid id, string contrasena)
    {
        var resultado = await _usuarioService.ReiniciarContrasenaAsync(id, contrasena);
        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = "Contraseña restablecida correctamente";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SucursalDto>> ObtenerSucursalesAsync()
    {
        var resultado = await _sucursalService.ObtenerTodosAsync(true);
        return resultado.IsSuccess ? resultado.Value! : new List<SucursalDto>();
    }
}