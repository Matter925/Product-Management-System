namespace ProductManagement.EFCore.Abstractions;

/// <summary>
/// Abstraction of Unit Of Work pattern
/// </summary>
public interface IUnitOfWork2
{
    Task<int> CompleteAsync();
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
}