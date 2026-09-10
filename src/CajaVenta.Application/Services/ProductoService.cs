using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repository;

    public ProductoService(IProductoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductoDto>> ObtenerPorIdAsync(Guid id)
    {
        var producto = await _repository.ObtenerPorIdAsync(id);
        if (producto is null)
            return Result<ProductoDto>.Failure("Producto no encontrado");

        return Result<ProductoDto>.Success(MapearADto(producto));
    }

    public async Task<Result<ProductoDto>> ObtenerPorCodigoBarrasAsync(string codigoBarras)
    {
        var producto = await _repository.ObtenerPorCodigoBarrasAsync(codigoBarras);
        if (producto is null)
            return Result<ProductoDto>.Failure("Producto no encontrado");

        return Result<ProductoDto>.Success(MapearADto(producto));
    }

    public async Task<Result<List<ProductoDto>>> ObtenerTodosAsync()
    {
        var productos = await _repository.ObtenerTodosAsync();
        return Result<List<ProductoDto>>.Success(productos.Select(MapearADto).ToList());
    }

    public async Task<Result<List<ProductoDto>>> BuscarAsync(string termino)
    {
        var productos = await _repository.BuscarAsync(termino);
        return Result<List<ProductoDto>>.Success(productos.Select(MapearADto).ToList());
    }

    public async Task<Result<ProductoDto>> CrearAsync(CrearProductoDto dto)
    {
        if (await _repository.ExisteCodigoBarrasAsync(dto.CodigoBarras))
            return Result<ProductoDto>.Failure("Ya existe un producto con ese código de barras");

        if (dto.Costo < 0)
            return Result<ProductoDto>.Failure("El costo no puede ser negativo");

        if (dto.PrecioVenta < 0)
            return Result<ProductoDto>.Failure("El precio de venta no puede ser negativo");

        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            CodigoBarras = dto.CodigoBarras,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Costo = dto.Costo,
            PrecioVenta = dto.PrecioVenta,
            TipoImpuesto = dto.TipoImpuesto,
            StockMinimo = dto.StockMinimo,
            PermiteDecimales = dto.PermiteDecimales,
            Categoria = dto.Categoria,
            UnidadMedida = dto.UnidadMedida
        };

        await _repository.CrearAsync(producto);
        return Result<ProductoDto>.Success(MapearADto(producto));
    }

    public async Task<Result<ProductoDto>> ActualizarAsync(Guid id, ActualizarProductoDto dto)
    {
        var producto = await _repository.ObtenerPorIdAsync(id);
        if (producto is null)
            return Result<ProductoDto>.Failure("Producto no encontrado");

        if (dto.Nombre is not null)
            producto.Nombre = dto.Nombre;
        if (dto.Descripcion is not null)
            producto.Descripcion = dto.Descripcion;
        if (dto.Costo.HasValue)
            producto.Costo = dto.Costo.Value;
        if (dto.PrecioVenta.HasValue)
            producto.PrecioVenta = dto.PrecioVenta.Value;
        if (dto.TipoImpuesto.HasValue)
            producto.TipoImpuesto = dto.TipoImpuesto.Value;
        if (dto.StockMinimo.HasValue)
            producto.StockMinimo = dto.StockMinimo.Value;
        if (dto.PermiteDecimales.HasValue)
            producto.PermiteDecimales = dto.PermiteDecimales.Value;
        if (dto.Categoria is not null)
            producto.Categoria = dto.Categoria;
        if (dto.UnidadMedida is not null)
            producto.UnidadMedida = dto.UnidadMedida;

        await _repository.ActualizarAsync(producto);
        return Result<ProductoDto>.Success(MapearADto(producto));
    }

    public async Task<Result<bool>> EliminarAsync(Guid id)
    {
        var producto = await _repository.ObtenerPorIdAsync(id);
        if (producto is null)
            return Result<bool>.Failure("Producto no encontrado");

        producto.Activo = false;
        await _repository.ActualizarAsync(producto);
        return Result<bool>.Success(true);
    }

    private static ProductoDto MapearADto(Producto p)
    {
        var margen = p.Costo > 0
            ? Math.Round((p.PrecioVenta - p.Costo) / p.Costo * 100, 2)
            : 0;

        return new ProductoDto
        {
            Id = p.Id,
            CodigoBarras = p.CodigoBarras,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Costo = p.Costo,
            PrecioVenta = p.PrecioVenta,
            TipoImpuesto = p.TipoImpuesto,
            StockMinimo = p.StockMinimo,
            PermiteDecimales = p.PermiteDecimales,
            Categoria = p.Categoria,
            UnidadMedida = p.UnidadMedida,
            Margen = margen
        };
    }

    private static ProductoBuscarDto MapearABuscarDto(Producto p) => new()
    {
        CodigoBarras = p.CodigoBarras,
        Nombre = p.Nombre,
        PrecioVenta = p.PrecioVenta,
        TipoImpuesto = p.TipoImpuesto,
        PermiteDecimales = p.PermiteDecimales,
        Variantes = p.Variantes.Select(v => new VarianteDto
        {
            Id = v.Id,
            Talla = v.Talla,
            Color = v.Color,
            Presentacion = v.Presentacion,
            SKU = v.SKU,
            PrecioAdicional = v.PrecioAdicional,
            StockActual = v.StockActual
        }).ToList()
    };
}
