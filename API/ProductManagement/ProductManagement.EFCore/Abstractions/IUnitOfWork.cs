using Microsoft.EntityFrameworkCore.Storage;

namespace ProductManagement.EFCore.Abstractions;

/// <summary>
/// Abstraction of Unit Of Work pattern
/// </summary>
public interface IUnitOfWork
{
    Task<int> CompleteAsync();
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<IDbContextTransaction> BeginTransactionAsync();
 }