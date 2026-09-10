using System.Data;
using CajaVenta.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CajaVenta.Infrastructure.Persistence;

public static class EsquemaMigracion
{
    private const string SUCURSAL_PRINCIPAL_ID = "4556A2B0-0E51-4E6B-9F3E-000000000001";
    private const string CAJA_PRINCIPAL_ID = "4556A2B0-0E51-4E6B-9F3E-000000000002";

    public static void Aplicar(CajaVentaDbContext context)
    {
        AplicarColumnasVentas(context);
        CrearTablaClientes(context);
        context.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS \"IX_Ventas_ClienteId\" ON \"Ventas\" (\"ClienteId\")");

        AplicarColumna(context, "Usuarios", "SucursalId");
        AplicarColumna(context, "TurnosCaja", "SucursalId");
        AplicarColumna(context, "MovimientosInventario", "SucursalId");
        AplicarColumna(context, "TurnosCaja", "CajaId");

        CrearTablasSucursales(context);
        BackfillSucursales(context);
        CrearRolesPermisos(context);
        CrearTablaCortesZ(context);
        NormalizarGuids(context);
    }

    private static void AplicarColumnasVentas(CajaVentaDbContext context)
    {
        if (!TieneColumna(context, "Ventas", "ClienteId"))
        {
            EjecutarConConexion(context, "ALTER TABLE \"Ventas\" ADD COLUMN \"ClienteId\" TEXT NULL;");
        }
    }

    private static void AplicarColumna(CajaVentaDbContext context, string tabla, string columna)
    {
        if (!TieneColumna(context, tabla, columna))
        {
            EjecutarConConexion(context, $"ALTER TABLE \"{tabla}\" ADD COLUMN \"{columna}\" TEXT NULL;");
        }
    }

    private static void CrearTablaClientes(CajaVentaDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "Clientes" (
                "Id" TEXT NOT NULL PRIMARY KEY,
                "NombreRazonSocial" TEXT NOT NULL,
                "NombreComercial" TEXT NULL,
                "RFC" TEXT NOT NULL,
                "Email" TEXT NULL,
                "Telefono" TEXT NULL,
                "CodigoPostal" TEXT NULL,
                "RegimenFiscal" TEXT NULL,
                "UsoCFDI" TEXT NULL,
                "ConstanciaRuta" TEXT NULL,
                "ConstanciaOriginalNombre" TEXT NULL,
                "Notas" TEXT NULL,
                "FechaCreacion" TEXT NOT NULL,
                "FechaModificacion" TEXT NULL,
                "Activo" INTEGER NOT NULL DEFAULT 1,
                "CreadoPor" TEXT NULL,
                "ModificadoPor" TEXT NULL
            )
            """);

        context.Database.ExecuteSqlRaw(
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Clientes_RFC\" ON \"Clientes\" (\"RFC\")");
    }

    private static void CrearTablasSucursales(CajaVentaDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "Sucursales" (
                "Id" TEXT NOT NULL PRIMARY KEY,
                "Nombre" TEXT NOT NULL,
                "Direccion" TEXT NULL,
                "Telefono" TEXT NULL,
                "Notas" TEXT NULL,
                "FechaCreacion" TEXT NOT NULL,
                "FechaModificacion" TEXT NULL,
                "Activo" INTEGER NOT NULL DEFAULT 1,
                "CreadoPor" TEXT NULL,
                "ModificadoPor" TEXT NULL
            )
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "Cajas" (
                "Id" TEXT NOT NULL PRIMARY KEY,
                "SucursalId" TEXT NOT NULL,
                "Nombre" TEXT NOT NULL,
                "Notas" TEXT NULL,
                "FechaCreacion" TEXT NOT NULL,
                "FechaModificacion" TEXT NULL,
                "Activo" INTEGER NOT NULL DEFAULT 1,
                "CreadoPor" TEXT NULL,
                "ModificadoPor" TEXT NULL
            )
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "StocksInventario" (
                "ProductoId" TEXT NOT NULL,
                "SucursalId" TEXT NOT NULL,
                "StockActual" REAL NOT NULL,
                PRIMARY KEY ("ProductoId", "SucursalId")
            )
            """);

        context.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS \"IX_Sucursales_Nombre\" ON \"Sucursales\" (\"Nombre\")");
        context.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS \"IX_Cajas_SucursalId\" ON \"Cajas\" (\"SucursalId\")");
        context.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS \"IX_Usuarios_SucursalId\" ON \"Usuarios\" (\"SucursalId\")");
        context.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS \"IX_TurnosCaja_CajaId\" ON \"TurnosCaja\" (\"CajaId\")");
        context.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS \"IX_TurnosCaja_SucursalId\" ON \"TurnosCaja\" (\"SucursalId\")");
        context.Database.ExecuteSqlRaw(
            "CREATE INDEX IF NOT EXISTS \"IX_MovimientosInventario_SucursalId\" ON \"MovimientosInventario\" (\"SucursalId\")");
    }

    private static void BackfillSucursales(CajaVentaDbContext context)
    {
        var ahora = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        if (Conteo(context, "Sucursales") == 0)
        {
            context.Database.ExecuteSqlRaw(
                "INSERT INTO \"Sucursales\" (\"Id\", \"Nombre\", \"Notas\", \"FechaCreacion\", \"Activo\") " +
                $"VALUES ('{SUCURSAL_PRINCIPAL_ID}', 'Sucursal Principal', 'Creada por migración', '{ahora}', 1)");
        }

        if (Conteo(context, "Cajas") == 0)
        {
            context.Database.ExecuteSqlRaw(
                "INSERT INTO \"Cajas\" (\"Id\", \"SucursalId\", \"Nombre\", \"Notas\", \"FechaCreacion\", \"Activo\") " +
                $"VALUES ('{CAJA_PRINCIPAL_ID}', '{SUCURSAL_PRINCIPAL_ID}', 'Caja 1', 'Creada por migración', '{ahora}', 1)");
        }

        context.Database.ExecuteSqlRaw(
            $"UPDATE \"Usuarios\" SET \"SucursalId\" = '{SUCURSAL_PRINCIPAL_ID}' " +
            "WHERE \"SucursalId\" IS NULL AND COALESCE(\"Rol\", 'Cajero') <> 'Admin'");

        context.Database.ExecuteSqlRaw(
            $"UPDATE \"TurnosCaja\" SET \"SucursalId\" = COALESCE(\"SucursalId\", '{SUCURSAL_PRINCIPAL_ID}'), " +
            $"\"CajaId\" = COALESCE(\"CajaId\", '{CAJA_PRINCIPAL_ID}') " +
            "WHERE \"SucursalId\" IS NULL OR \"CajaId\" IS NULL");

        context.Database.ExecuteSqlRaw(
            $"UPDATE \"MovimientosInventario\" SET \"SucursalId\" = '{SUCURSAL_PRINCIPAL_ID}' " +
            "WHERE \"SucursalId\" IS NULL");

        if (TieneColumna(context, "Productos", "StockActual") && Conteo(context, "StocksInventario") == 0)
        {
            context.Database.ExecuteSqlRaw(
                $"INSERT INTO \"StocksInventario\" (\"ProductoId\", \"SucursalId\", \"StockActual\") " +
                $"SELECT \"Id\", '{SUCURSAL_PRINCIPAL_ID}', COALESCE(\"StockActual\", 0) FROM \"Productos\"");
        }
    }

    private static void CrearTablaCortesZ(CajaVentaDbContext context)
    {
        if (TieneTabla(context, "CortesZ"))
            return;

        EjecutarConConexion(context, """
            CREATE TABLE "CortesZ" (
                "Id" TEXT NOT NULL PRIMARY KEY,
                "Numero" INTEGER NOT NULL,
                "TurnoCajaId" TEXT NOT NULL,
                "SucursalId" TEXT NOT NULL,
                "CajaId" TEXT NOT NULL,
                "UsuarioId" TEXT NOT NULL,
                "FechaApertura" TEXT NOT NULL,
                "FechaCierre" TEXT NOT NULL,
                "FondoInicial" TEXT NOT NULL,
                "FondoFinal" TEXT NULL,
                "Subtotal" TEXT NOT NULL,
                "Impuestos" TEXT NOT NULL,
                "TotalVentas" TEXT NOT NULL,
                "CantidadVentas" INTEGER NOT NULL,
                "CantidadCanceladas" INTEGER NOT NULL,
                "VentasEfectivo" TEXT NOT NULL,
                "VentasTarjeta" TEXT NOT NULL,
                "VentasOtros" TEXT NOT NULL,
                "FechaCreacion" TEXT NOT NULL,
                "FechaModificacion" TEXT NULL,
                "Activo" INTEGER NOT NULL DEFAULT 1,
                "CreadoPor" TEXT NULL,
                "ModificadoPor" TEXT NULL
            )
            """);

        EjecutarConConexion(context,
            "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_CortesZ_CajaId_Numero\" ON \"CortesZ\" (\"CajaId\", \"Numero\")");
        EjecutarConConexion(context,
            "CREATE INDEX IF NOT EXISTS \"IX_CortesZ_SucursalId\" ON \"CortesZ\" (\"SucursalId\")");
        EjecutarConConexion(context,
            "CREATE INDEX IF NOT EXISTS \"IX_CortesZ_CajaId\" ON \"CortesZ\" (\"CajaId\")");
        EjecutarConConexion(context,
            "CREATE INDEX IF NOT EXISTS \"IX_CortesZ_TurnoCajaId\" ON \"CortesZ\" (\"TurnoCajaId\")");
    }

    private static bool TieneColumna(CajaVentaDbContext context, string tabla, string columna)
    {
        var connection = context.Database.GetDbConnection();
        var estabaCerrada = connection.State != ConnectionState.Open;
        if (estabaCerrada)
            connection.Open();

        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info(\"{tabla}\")";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader["name"]?.ToString(), columna, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        finally
        {
            if (estabaCerrada)
                connection.Close();
        }
    }

    private static int Conteo(CajaVentaDbContext context, string tabla)
    {
        var resultado = EjecutarEscalarConConexion(context, $"SELECT COUNT(*) FROM \"{tabla}\"");
        return Convert.ToInt32(resultado);
    }

    private static object? EjecutarEscalarConConexion(CajaVentaDbContext context, string sql)
    {
        var connection = context.Database.GetDbConnection();
        var estabaCerrada = connection.State != ConnectionState.Open;
        if (estabaCerrada)
            connection.Open();

        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteScalar();
        }
        finally
        {
            if (estabaCerrada)
                connection.Close();
        }
    }

    private static void EjecutarConConexion(CajaVentaDbContext context, string sql)
    {
        var connection = context.Database.GetDbConnection();
        var estabaCerrada = connection.State != ConnectionState.Open;
        if (estabaCerrada)
            connection.Open();

        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
        finally
        {
            if (estabaCerrada)
                connection.Close();
        }
    }

    private static bool TieneTabla(CajaVentaDbContext context, string tabla)
    {
        var result = EjecutarEscalarConConexion(context, $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tabla}'");
        return Convert.ToInt32(result) > 0;
    }

    private static void CrearRolesPermisos(CajaVentaDbContext context)
    {
        EjecutarConConexion(context, @"CREATE TABLE IF NOT EXISTS ""RolesPermisos"" (
            ""Rol"" TEXT NOT NULL,
            ""PermisoKey"" TEXT NOT NULL,
            PRIMARY KEY(""Rol"", ""PermisoKey"")
        );");

        if (Conteo(context, "RolesPermisos") > 0)
            return;

        foreach (var clave in Permisos.Todos)
            EjecutarConConexion(context, $"INSERT OR IGNORE INTO \"RolesPermisos\" (\"Rol\",\"PermisoKey\") VALUES ('Admin','{clave}');");

        foreach (var clave in Permisos.PredeterminadosCajero)
            EjecutarConConexion(context, $"INSERT OR IGNORE INTO \"RolesPermisos\" (\"Rol\",\"PermisoKey\") VALUES ('Cajero','{clave}');");
    }

    private static void NormalizarGuids(CajaVentaDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var estabaCerrada = connection.State != ConnectionState.Open;
        if (estabaCerrada)
            connection.Open();

        try
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var tabla = reader["name"]?.ToString();
                    if (string.IsNullOrEmpty(tabla))
                        continue;

                    var columnas = new List<string>();
                    using (var info = connection.CreateCommand())
                    {
                        info.CommandText = $"PRAGMA table_info(\"{tabla}\")";
                        using var infoReader = info.ExecuteReader();
                        while (infoReader.Read())
                        {
                            var columna = infoReader["name"]?.ToString();
                            if (string.IsNullOrEmpty(columna))
                                continue;
                            columnas.Add(columna);
                        }
                    }

                    foreach (var columna in columnas)
                    {
                        using var update = connection.CreateCommand();
                        update.CommandText = $"""
                            UPDATE "{tabla}"
                            SET "{columna}" = UPPER("{columna}")
                            WHERE length("{columna}") = 36
                              AND "{columna}" NOT GLOB '*[^0-9A-Fa-f-]*'
                              AND "{columna}" <> UPPER("{columna}");
                            """;
                        update.ExecuteNonQuery();
                    }
                }
            }
        }
        finally
        {
            if (estabaCerrada)
                connection.Close();
        }
    }
}