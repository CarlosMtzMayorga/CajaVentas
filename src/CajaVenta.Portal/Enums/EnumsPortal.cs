namespace CajaVenta.Portal.Enums;

public enum EstadoSuscriptor
{
    Activo = 0,
    Suspendido = 1,
    Cancelado = 2,
    Prueba = 3
}

public enum EstadoSuscripcion
{
    Activa = 0,
    Pendiente = 1,
    Vencida = 2,
    Cancelada = 3
}

public enum EstadoPago
{
    Pagado = 0,
    Pendiente = 1,
    Rechazado = 2,
    Reembolsado = 3
}

public enum MetodoPagoPasarela
{
    Tarjeta = 0,
    Transferencia = 1,
    Stripe = 2,
    MercadoPago = 3
}