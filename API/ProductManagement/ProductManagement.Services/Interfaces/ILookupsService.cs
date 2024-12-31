using ProductManagement.EFCore.Shared;

namespace ProductManagement.Services.Interfaces;

public interface ILookupsService
{
    List<Lookups> GetAllAsync(string tableName);
    Task<Lookups?> GetByIdAsync(string tableName, int id);
    Task<string?> CreateAsync(string userId, string tableName, LookupsEdit lookups);
    Task<string?> UpdateAsync(string userId, string tableName, int id, LookupsEdit lookups);
    Task<string?> DeleteAsync(string userId, string tableName, int id);
    IEnumerable<string> GetTableNames(string schemaName);
}
