using ConexaStarWars.Core.Entities;

namespace ConexaStarWars.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Movie> Movies { get; }
    IRepository<User> Users { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}