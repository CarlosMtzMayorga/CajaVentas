using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Common;
using CajaVenta.Web.Models;
using CajaVenta.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
[Permiso(Permisos.GestionarSucursales)]
public class SucursalesController : Controller
{
    private readonly ISucursalService _sucursalService;

    public SucursalesController(ISucursalService sucursalService)
        => _sucursalService = sucursalService;

    public async Task<IActionResult> Index()
    {
        var resultado = await _sucursalService.ObtenerTodosAsync();
        var modelo = new SucursalesIndexModel
        {
            Sucursales = resultado.IsSuccess ? resultado.Value! : new List<SucursalDto>(),
            Error = resultado.IsFailure ? resultado.Error : null,
            Mensaje = TempData["Mensaje"] as string
        };
        return View(modelo);
    }

    public IActionResult Crear()
        => View(new CrearSucursalDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearSucursalDto modelo)
    {
        var resultado = await _sucursalService.CrearAsync(modelo);
        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Mensaje"] = $"Sucursal creada: {resultado.Value!.Nombre}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(Guid id)
    {
        var resultado = await _sucursalService.ObtenerPorIdAsync(id);
        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        var s = resultado.Value!;
        return View(new EditarSucursalModel
        {
            Id = s.Id,
            Nombre = s.Nombre,
            Direccion = s.Direccion,
            Telefono = s.Telefono,
            Notas = s.Notas
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, EditarSucursalModel modelo)
    {
        var resultado = await _sucursalService.ActualizarAsync(id, new ActualizarSucursalDto
        {
            Nombre = modelo.Nombre,
            Direccion = modelo.Direccion,
            Telefono = modelo.Telefono,
            Notas = modelo.Notas
        });

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Mensaje"] = "Sucursal actualizada correctamente";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarActivo(Guid id, bool activo)
    {
        var resultado = await _sucursalService.ActivarAsync(id, activo);
        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = activo ? "Sucursal activada" : "Sucursal desactivada";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Cajas(Guid sucursalId)
    {
        var sucursal = await _sucursalService.ObtenerPorIdAsync(sucursalId);
        if (sucursal.IsFailure)
        {
            TempData["Error"] = sucursal.Error;
            return RedirectToAction(nameof(Index));
        }

        var cajas = await _sucursalService.ObtenerCajasAsync(sucursalId);
        var modelo = new CajasIndexModel
        {
            Sucursal = sucursal.Value!,
            Cajas = cajas.IsSuccess ? cajas.Value! : new List<CajaDto>(),
            Error = cajas.IsFailure ? cajas.Error : null,
            Mensaje = TempData["Mensaje"] as string
        };
        return View(modelo);
    }

    public IActionResult CrearCaja(Guid sucursalId)
        => View(new CrearCajaDto { SucursalId = sucursalId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCaja(CrearCajaDto modelo)
    {
        var resultado = await _sucursalService.CrearCajaAsync(modelo);
        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Cajas), new { sucursalId = modelo.SucursalId });
        }

        TempData["Mensaje"] = $"Caja creada: {resultado.Value!.Nombre}";
        return RedirectToAction(nameof(Cajas), new { sucursalId = modelo.SucursalId });
    }

    public async Task<IActionResult> EditarCaja(Guid cajaId)
    {
        var caja = await BuscarCajaAsync(cajaId);
        if (caja is null)
        {
            TempData["Error"] = "Caja no encontrada";
            return RedirectToAction(nameof(Index));
        }

        return View(new EditarCajaModel
        {
            Id = caja.Id,
            SucursalId = caja.SucursalId,
            Nombre = caja.Nombre,
            Notas = caja.Notas
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCaja(Guid cajaId, EditarCajaModel modelo)
    {
        var resultado = await _sucursalService.ActualizarCajaAsync(cajaId, new ActualizarCajaDto
        {
            Nombre = modelo.Nombre,
            Notas = modelo.Notas
        });

        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(EditarCaja), new { cajaId });
        }

        TempData["Mensaje"] = "Caja actualizada correctamente";
        return RedirectToAction(nameof(Cajas), new { sucursalId = resultado.Value!.SucursalId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarActivoCaja(Guid cajaId, bool activo, Guid sucursalId)
    {
        var resultado = await _sucursalService.ActivarCajaAsync(cajaId, activo);
        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = activo ? "Caja activada" : "Caja desactivada";
        return RedirectToAction(nameof(Cajas), new { sucursalId });
    }

    private async Task<CajaDto?> BuscarCajaAsync(Guid cajaId)
    {
        var sucursales = await _sucursalService.ObtenerTodosAsync();
        if (sucursales.IsFailure)
            return null;

        foreach (var s in sucursales.Value!)
        {
            var cajas = await _sucursalService.ObtenerCajasAsync(s.Id);
            if (!cajas.IsSuccess)
                continue;
            var caja = cajas.Value!.FirstOrDefault(c => c.Id == cajaId);
            if (caja is not null)
                return caja;
        }
        return null;
    }
}