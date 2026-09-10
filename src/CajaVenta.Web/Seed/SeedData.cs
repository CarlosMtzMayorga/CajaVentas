using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Application.Security;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Web.Seed;

public static class SeedData
{
    private static readonly Guid SucursalPrincipalId = Guid.Parse("4556a2b0-0e51-4e6b-9f3e-000000000001");
    private static readonly Guid CajaPrincipalId = Guid.Parse("4556a2b0-0e51-4e6b-9f3e-000000000002");

    public static void Inicializar(CajaVentaDbContext context, IServiceProvider services)
    {
        if (context.Usuarios.Any())
            return;

        var sucursal = CrearSucursalPrincipal(context);
        var caja = CrearCajaPrincipal(context, sucursal.Id);

        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Administrador",
            NombreUsuario = "admin",
            ContrasenaHash = PasswordHasher.Hash("admin123"),
            Email = "admin@cajaventa.local",
            Rol = "Admin",
            Activo = true
        };

        var cajero = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Cajero General",
            NombreUsuario = "cajero",
            ContrasenaHash = PasswordHasher.Hash("cajero123"),
            Email = "cajero@cajaventa.local",
            Rol = "Cajero",
            SucursalId = sucursal.Id,
            Activo = true
        };

        context.Usuarios.AddRange(admin, cajero);
        context.SaveChanges();

        var productoService = services.GetRequiredService<IProductoService>();

        var productos = new[]
        {
            new CrearProductoDto { CodigoBarras = "7501020515001", Nombre = "Coca-Cola 600ml", Costo = 8.50m, PrecioVenta = 16.00m, TipoImpuesto = TipoImpuesto.IVA16, StockMinimo = 10, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515002", Nombre = "Pepsi 600ml", Costo = 8.00m, PrecioVenta = 15.00m, TipoImpuesto = TipoImpuesto.IVA16, StockMinimo = 10, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515003", Nombre = "Sabritas Original", Costo = 12.00m, PrecioVenta = 22.00m, TipoImpuesto = TipoImpuesto.IVA16, StockMinimo = 15, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515004", Nombre = "Gansito Marinela", Costo = 6.00m, PrecioVenta = 12.00m, TipoImpuesto = TipoImpuesto.IVA16, StockMinimo = 20, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515005", Nombre = "Leche Lala 1L", Costo = 18.00m, PrecioVenta = 28.00m, TipoImpuesto = TipoImpuesto.Exento, StockMinimo = 10, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515006", Nombre = "Pan Bimbo Blanco", Costo = 22.00m, PrecioVenta = 38.00m, TipoImpuesto = TipoImpuesto.Exento, StockMinimo = 8, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515007", Nombre = "Jabón Dove", Costo = 28.00m, PrecioVenta = 45.00m, TipoImpuesto = TipoImpuesto.IVA16, StockMinimo = 5, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515008", Nombre = "Cerveza Corona Extra", Costo = 12.00m, PrecioVenta = 20.00m, TipoImpuesto = TipoImpuesto.IVA8Frontera, StockMinimo = 24, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515009", Nombre = "Atún Tuna 140g", Costo = 14.00m, PrecioVenta = 24.00m, TipoImpuesto = TipoImpuesto.Exento, StockMinimo = 12, UnidadMedida = "pza" },
            new CrearProductoDto { CodigoBarras = "7501020515010", Nombre = "Café Nescafé 100g", Costo = 42.00m, PrecioVenta = 65.00m, TipoImpuesto = TipoImpuesto.Exento, StockMinimo = 8, UnidadMedida = "pza" },
        };

        foreach (var prod in productos)
        {
            var resultado = productoService.CrearAsync(prod).GetAwaiter().GetResult();
            if (resultado.IsSuccess)
            {
                context.MovimientosInventario.Add(new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    ProductoId = resultado.Value!.Id,
                    SucursalId = sucursal.Id,
                    Tipo = TipoMovimientoInventario.Entrada,
                    Cantidad = prod.StockMinimo * 3,
                    CostoUnitario = prod.Costo,
                    Referencia = "SEED-INITIAL",
                    Fecha = DateTime.UtcNow
                });
            }
        }

        context.SaveChanges();

        foreach (var prod in context.Productos.ToList())
        {
            var movimientos = context.MovimientosInventario
                .Where(m => m.ProductoId == prod.Id)
                .ToList()
                .Sum(m => m.Cantidad);

            context.StocksInventario.Add(new StockInventario
            {
                ProductoId = prod.Id,
                SucursalId = sucursal.Id,
                StockActual = movimientos
            });
        }
        context.SaveChanges();

        _ = caja;
    }

    private static Sucursal CrearSucursalPrincipal(CajaVentaDbContext context)
    {
        var sucursal = context.Sucursales.FirstOrDefault(s => s.Id == SucursalPrincipalId);
        if (sucursal is null)
        {
            sucursal = new Sucursal
            {
                Id = SucursalPrincipalId,
                Nombre = "Sucursal Principal",
                Notas = "Creada por seed",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            context.Sucursales.Add(sucursal);
            context.SaveChanges();
        }
        return sucursal;
    }

    private static Caja CrearCajaPrincipal(CajaVentaDbContext context, Guid sucursalId)
    {
        var caja = context.Cajas.FirstOrDefault(c => c.Id == CajaPrincipalId);
        if (caja is null)
        {
            caja = new Caja
            {
                Id = CajaPrincipalId,
                SucursalId = sucursalId,
                Nombre = "Caja 1",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            context.Cajas.Add(caja);
            context.SaveChanges();
        }
        return caja;
    }
}