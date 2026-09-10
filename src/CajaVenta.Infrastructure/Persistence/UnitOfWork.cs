using CajaVenta.Application.Common;
using CajaVenta.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CajaVenta.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly CajaVentaDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(CajaVentaDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}