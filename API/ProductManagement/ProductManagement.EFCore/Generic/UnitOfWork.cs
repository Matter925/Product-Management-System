using Microsoft.EntityFrameworkCore.Storage;

using ProductManagement.EFCore.Models;

using ProductManagement.EFCore.Abstractions;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.EFCore.Generic;

/// <summary>
/// Implementation of Unit of work pattern
/// </summary>
public class UnitOfWork(ProductManagementDBContext ProductManagementDBContext) : IUnitOfWork
{
    private readonly ProductManagementDBContext Context = ProductManagementDBContext;
    private readonly Dictionary<Type, object> Repositories = new Dictionary<Type, object>();

    public async Task<int> CompleteAsync()
    {
        try
        {
            return await Context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (!Repositories.TryGetValue(type, out object? value))
        {
            var repository = new Repository<TEntity>(Context);
            value = repository;
            Repositories.Add(type, value);
        }

        return (IRepository<TEntity>)value;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
    }
}
