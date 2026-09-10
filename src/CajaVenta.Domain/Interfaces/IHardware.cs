namespace CajaVenta.Domain.Interfaces;

public interface ITicketPrinter
{
    Task<bool> ImprimirTicketAsync(string contenido);
    Task<bool> EstaConectadaAsync();
    void AbrirCajon();
}

public interface IScaleReader
{
    Task<decimal?> LeerPesoAsync();
    bool EstaConectadaAsync();
}

public interface ICashDrawer
{
    Task AbrirAsync();
    bool EstaConectadoAsync();
}
