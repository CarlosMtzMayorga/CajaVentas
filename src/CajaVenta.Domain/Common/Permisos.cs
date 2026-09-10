namespace CajaVenta.Domain.Common;

public static class Permisos
{
    public const string PuntoVenta = nameof(PuntoVenta);
    public const string Turnos = nameof(Turnos);
    public const string VerProductos = nameof(VerProductos);
    public const string GestionarProductos = nameof(GestionarProductos);
    public const string VerInventarios = nameof(VerInventarios);
    public const string RegistrarMovimientosInventario = nameof(RegistrarMovimientosInventario);
    public const string VerClientes = nameof(VerClientes);
    public const string GestionarClientes = nameof(GestionarClientes);
    public const string VerReportes = nameof(VerReportes);
    public const string GestionarSucursales = nameof(GestionarSucursales);
    public const string GestionarUsuarios = nameof(GestionarUsuarios);
    public const string Configuracion = nameof(Configuracion);
    public const string Permisologia = nameof(Permisologia);

    public static readonly string[] Todos =
    [
        PuntoVenta, Turnos,
        VerProductos, GestionarProductos,
        VerInventarios, RegistrarMovimientosInventario,
        VerClientes, GestionarClientes,
        VerReportes,
        GestionarSucursales, GestionarUsuarios,
        Configuracion, Permisologia,
    ];

    public static readonly string[] PredeterminadosCajero =
    [
        PuntoVenta, Turnos,
        VerProductos,
        VerInventarios,
        VerClientes, GestionarClientes,
        VerReportes,
    ];

    public static readonly (string Clave, string Nombre, string Modulo, string Descripcion)[] Catalogo =
    [
        (PuntoVenta, "Punto de venta", "Ventas", "Cobrar en el POS, carrito y tickets."),
        (Turnos, "Turnos de caja", "Ventas", "Abrir, cerrar y ver corte de caja."),
        (VerReportes, "Ver reportes", "Reportes", "Consultar ventas y stock por sucursal."),
        (VerProductos, "Ver productos", "Catálogos", "Consultar el catálogo de productos."),
        (GestionarProductos, "Gestionar productos", "Catálogos", "Crear, editar y eliminar productos."),
        (VerInventarios, "Ver inventarios", "Inventario", "Consultar existencias y movimientos."),
        (RegistrarMovimientosInventario, "Registrar movimientos", "Inventario", "Realizar entradas, salidas y ajustes de stock."),
        (VerClientes, "Ver clientes", "Clientes", "Consultar el catálogo de clientes."),
        (GestionarClientes, "Gestionar clientes", "Clientes", "Crear y editar clientes."),
        (GestionarSucursales, "Gestionar sucursales", "Configuración", "Sucursales y cajas."),
        (GestionarUsuarios, "Gestionar usuarios", "Configuración", "Dar de alta y modificar usuarios."),
        (Configuracion, "Configuración general", "Configuración", "Acceso al menú de configuración."),
        (Permisologia, "Permisología", "Configuración", "Ver y editar permisos por rol."),
    ];
}