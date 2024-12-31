using System.Linq.Expressions;

using ProductManagement.EFCore.ResourceParams;

namespace ProductManagement.Services.Interfaces;

public interface IBaseService<TModel> where TModel : class
{
    Task<IEnumerable<TDto>> GetAllAsync<TDto>(bool asNoTracking = true, ResourceParams? resourceParams = null, bool ignorePaging = false, Expression<Func<TModel, TDto>>? selector = null, string columnName = "Id", bool asSplitQuery = false) where TDto : new();
    Task<TModel?> GetByIdAsync(int id, bool asNoTracking = true, string columnName = "Id", List<string>? includedProperties = null, bool asSplitQuery = false);
    Task<TDto?> GetByIdAsync<TDto>(int id, bool asNoTracking = true, string columnName = "Id", Expression<Func<TModel, TDto>>? selector = null, bool asSplitQuery = false) where TDto : new();
    Task<int?> CreateAsync(TModel model, string idColumnName = "Id");
    Task<bool> UpdateAsync(int id, TModel model, bool usePartialUpdate = true, string idColumnName = "Id", bool directUpdate = false, Type? updateType = null, bool asSplitQuery = false);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteRangeAsync(List<TModel> entities);
    Task<bool> AnyAsync(string columnName, object value);
    Task<bool> PartialUpdateAsync(int id, TModel model, string idColumnName = "Id", Type? updateType = null);
}
