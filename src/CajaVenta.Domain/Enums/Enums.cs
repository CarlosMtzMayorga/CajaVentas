namespace CajaVenta.Domain.Enums;

public enum TipoImpuesto
{
    IVA16 = 0,
    IVA8Frontera = 1,
    Exento = 2,
    IEPS = 3
}

public enum TipoMovimientoInventario
{
    Entrada = 0,
    Salida = 1,
    Venta = 2,
    Ajuste = 3,
    Devolucion = 4
}

public enum MetodoPago
{
    Efectivo = 0,
    TarjetaCredito = 1,
    TarjetaDebito = 2,
    Transferencia = 3,
    QR = 4
}

public enum EstadoVenta
{
    Completada = 0,
    Cancelada = 1,
    Pendiente = 2
}

public enum EstadoTurno
{
    Abierto = 0,
    Cerrado = 1
}
