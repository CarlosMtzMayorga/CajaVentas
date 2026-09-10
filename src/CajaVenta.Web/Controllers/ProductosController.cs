using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Enums;
using CajaVenta.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
public class ProductosController : Controller
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
        => _productoService = productoService;

    public async Task<IActionResult> Index(string? q)
    {
        var resultado = string.IsNullOrWhiteSpace(q)
            ? await _productoService.ObtenerTodosAsync()
            : await _productoService.BuscarAsync(q.Trim());

        var modelo = new ProductosIndexModel
        {
            Busqueda = q,
            Productos = resultado.IsSuccess ? resultado.Value!.OrderBy(p => p.Nombre).ToList() : new List<ProductoDto>(),
            Error = resultado.IsFailure ? resultado.Error : null,
            Mensaje = TempData["Mensaje"] as string
        };

        return View(modelo);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Crear()
        => View(new CrearProductoDto());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearProductoDto modelo)
    {
        if (!Enum.IsDefined(modelo.TipoImpuesto))
            ModelState.AddModelError(nameof(modelo.TipoImpuesto), "Tipo de impuesto no válido");

        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _productoService.CrearAsync(modelo);
        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error ?? "No se pudo crear el producto");
            return View(modelo);
        }

        TempData["Mensaje"] = $"Producto creado: {resultado.Value!.Nombre}";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var resultado = await _productoService.ObtenerPorIdAsync(id);
        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        var producto = resultado.Value!;
        return View(new EditarProductoModel
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Costo = producto.Costo,
            PrecioVenta = producto.PrecioVenta,
            TipoImpuesto = producto.TipoImpuesto,
            StockMinimo = producto.StockMinimo,
            UnidadMedida = producto.UnidadMedida,
            Categoria = producto.Categoria
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, EditarProductoModel modelo)
    {
        var resultado = await _productoService.ActualizarAsync(id, new ActualizarProductoDto
        {
            Nombre = modelo.Nombre,
            Costo = modelo.Costo,
            PrecioVenta = modelo.PrecioVenta,
            TipoImpuesto = modelo.TipoImpuesto,
            StockMinimo = modelo.StockMinimo,
            UnidadMedida = modelo.UnidadMedida,
            Categoria = modelo.Categoria
        });

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error ?? "No se pudo actualizar el producto");
            return View(modelo);
        }

        TempData["Mensaje"] = $"Producto actualizado: {resultado.Value!.Nombre}";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var resultado = await _productoService.EliminarAsync(id);
        if (resultado.IsFailure)
            TempData["Error"] = resultado.Error;
        else
            TempData["Mensaje"] = "Producto eliminado";

        return RedirectToAction(nameof(Index));
    }
}