using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;
    private readonly IWebHostEnvironment _env;

    public ClientesController(IClienteService clienteService, IWebHostEnvironment env)
    {
        _clienteService = clienteService;
        _env = env;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var resultado = string.IsNullOrWhiteSpace(q)
            ? await _clienteService.ObtenerTodosAsync()
            : await _clienteService.BuscarAsync(q.Trim());

        var modelo = new ClientesIndexModel
        {
            Busqueda = q,
            Clientes = resultado.IsSuccess ? resultado.Value! : new List<ClienteDto>(),
            Error = resultado.IsFailure ? resultado.Error : null,
            Mensaje = TempData["Mensaje"] as string
        };

        return View(modelo);
    }

    public IActionResult Crear()
        => View(new ClienteFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ClienteFormModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        if (string.IsNullOrWhiteSpace(modelo.RFC))
            ModelState.AddModelError(nameof(modelo.RFC), "El RFC es obligatorio para poder facturar");

        if (!ModelState.IsValid)
            return View(modelo);

        var constancia = await GuardarConstanciaAsync(modelo.Constancia);
        if (constancia is null && modelo.Constancia is not null)
            return View(modelo);

        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _clienteService.CrearAsync(new CrearClienteDto
        {
            NombreRazonSocial = modelo.NombreRazonSocial,
            NombreComercial = modelo.NombreComercial,
            RFC = modelo.RFC,
            Email = modelo.Email,
            Telefono = modelo.Telefono,
            CodigoPostal = modelo.CodigoPostal,
            RegimenFiscal = modelo.RegimenFiscal,
            UsoCFDI = modelo.UsoCFDI,
            ConstanciaRuta = constancia?.RutaAlmacenada,
            ConstanciaOriginalNombre = constancia?.NombreOriginal,
            Notas = modelo.Notas
        });

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Mensaje"] = $"Cliente registrado: {resultado.Value!.RFC}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(Guid id)
    {
        var resultado = await _clienteService.ObtenerPorIdAsync(id);
        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        var c = resultado.Value!;
        return View(new ClienteFormModel
        {
            Id = c.Id,
            NombreRazonSocial = c.NombreRazonSocial,
            RFC = c.RFC,
            NombreComercial = c.NombreComercial,
            Email = c.Email,
            Telefono = c.Telefono,
            CodigoPostal = c.CodigoPostal,
            RegimenFiscal = c.RegimenFiscal,
            UsoCFDI = c.UsoCFDI,
            Notas = c.Notas,
            TieneConstancia = c.TieneConstancia,
            ConstanciaOriginalNombre = c.ConstanciaOriginalNombre
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, ClienteFormModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var actual = await _clienteService.ObtenerPorIdAsync(id);
        if (actual.IsFailure)
        {
            TempData["Error"] = actual.Error;
            return RedirectToAction(nameof(Index));
        }

        var constancia = await GuardarConstanciaAsync(modelo.Constancia);
        if (constancia is null && modelo.Constancia is not null)
            return View(modelo);

        var resultado = await _clienteService.ActualizarAsync(id, new ActualizarClienteDto
        {
            NombreRazonSocial = modelo.NombreRazonSocial,
            NombreComercial = modelo.NombreComercial,
            Email = modelo.Email,
            Telefono = modelo.Telefono,
            CodigoPostal = modelo.CodigoPostal,
            RegimenFiscal = modelo.RegimenFiscal,
            UsoCFDI = modelo.UsoCFDI,
            ConstanciaRuta = constancia?.RutaAlmacenada,
            ConstanciaOriginalNombre = constancia?.NombreOriginal,
            Notas = modelo.Notas
        });

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        if (constancia is not null && !string.IsNullOrWhiteSpace(actual.Value!.ConstanciaRuta))
            EliminarArchivoConstancia(actual.Value.ConstanciaRuta);

        TempData["Mensaje"] = "Cliente actualizado correctamente";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarActivo(Guid id, bool activo)
    {
        var resultado = await _clienteService.ActivarAsync(id, activo);
        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = activo ? "Cliente activado" : "Cliente desactivado";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DescargarConstancia(Guid id)
    {
        var resultado = await _clienteService.ObtenerPorIdAsync(id);
        if (resultado.IsFailure || string.IsNullOrWhiteSpace(resultado.Value!.ConstanciaRuta))
        {
            TempData["Error"] = "El cliente no tiene constancia de situación fiscal";
            return RedirectToAction(nameof(Index));
        }

        if (!System.IO.File.Exists(resultado.Value.ConstanciaRuta))
        {
            TempData["Error"] = "El archivo de la constancia ya no existe en el servidor";
            return RedirectToAction(nameof(Index));
        }

        var nombreDescarga = resultado.Value.ConstanciaOriginalNombre ?? $"constancia-{resultado.Value.RFC}.pdf";
        return PhysicalFile(resultado.Value.ConstanciaRuta, "application/octet-stream", nombreDescarga);
    }

    private async Task<(string RutaAlmacenada, string NombreOriginal)?> GuardarConstanciaAsync(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return null;

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var permitidas = new[] { ".pdf", ".xml", ".jpg", ".jpeg", ".png" };
        if (!permitidas.Contains(ext))
        {
            ModelState.AddModelError(nameof(ClienteFormModel.Constancia), "Formato no permitido. Usa PDF, XML, JPG o PNG.");
            return null;
        }

        if (archivo.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(nameof(ClienteFormModel.Constancia), "El archivo no puede superar 5 MB.");
            return null;
        }

        var dir = Path.Combine(_env.ContentRootPath, "App_Data", "Constancias");
        Directory.CreateDirectory(dir);
        var nombre = $"{Guid.NewGuid():N}{ext}";
        var ruta = Path.Combine(dir, nombre);

        await using (var stream = System.IO.File.Create(ruta))
        {
            await archivo.CopyToAsync(stream);
        }

        return (ruta, Path.GetFileName(archivo.FileName));
    }

    private void EliminarArchivoConstancia(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return;
        if (System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);
    }
}