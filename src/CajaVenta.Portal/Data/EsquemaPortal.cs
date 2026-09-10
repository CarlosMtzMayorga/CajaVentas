using CajaVenta.Application.Security;
using CajaVenta.Portal.Entities;

namespace CajaVenta.Portal.Data;

public static class EsquemaPortal
{
    public static void Sembrar(PortalDbContext context)
    {
        if (!context.UsuariosPortal.Any())
        {
            context.UsuariosPortal.Add(new UsuarioPortal
            {
                Id = Guid.NewGuid(),
                NombreUsuario = "admin",
                Nombre = "Administrador del Portal",
                ContrasenaHash = PasswordHasher.Hash("portal123"),
                Rol = "Admin",
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        var nombrePredefinidos = new[] { "Básico", "Pro", "Empresarial" };
        foreach (var nombre in nombrePredefinidos)
        {
            if (context.Planes.Any(p => p.Nombre == nombre))
                continue;

            var precio = nombre switch
            {
                "Básico" => (decimal)9.99,
                "Pro" => (decimal)19.99,
                _ => (decimal)39.99
            };

            context.Planes.Add(new Plan
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = $"{nombre} - plataforma de punto de venta",
                PrecioMensual = precio,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        context.SaveChanges();
    }
}