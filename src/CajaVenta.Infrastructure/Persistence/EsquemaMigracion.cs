using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CajaVenta.Infrastructure.Persistence;

public static class EsquemaMigracion
{
    private const string SUCURSAL_PRINCIPAL_ID = "4556a2b0-0e51-4e6b-9f3e-000000000001";
    private const string CAJA_PRINCIPAL_ID = "4556a2b0-0e51-4e6b-9f3e-000000000002";

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
}