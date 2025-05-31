using ConexaStarWars.Core.Entities;
using ConexaStarWars.Core.Interfaces;
using ConexaStarWars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ConexaStarWars.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IRepository<Movie>? _movies;
    private IDbContextTransaction? _transaction;
    private IRepository<User>? _users;

    public IRepository<Movie> Movies => _movies ??= new Repository<Movie>(context);
    public IRepository<User> Users => _users ??= new Repository<User>(context);

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}