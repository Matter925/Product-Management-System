using ProductManagement.Shared.Enums;

namespace ProductManagement.Services.Interfaces;
public interface IFilterByUserAndRole<T> where T : class
{
    public Task<IQueryable<T>> FilterDataByUserAndRoleAsync(IQueryable<T> queryable, Roles? role, string userId, int? Id = null);
}
