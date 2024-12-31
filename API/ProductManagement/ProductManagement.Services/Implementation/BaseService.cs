using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;

using AutoFilterer;

using CsvHelper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

using ProductManagement.EFCore.Abstractions;
using ProductManagement.EFCore.Models;
using ProductManagement.EFCore.Pagination;
using ProductManagement.EFCore.ResourceParams;
using ProductManagement.Services.Interfaces;
using ProductManagement.Shared.Enums;

namespace ProductManagement.Services.Implementation;
public class BaseService<TModel>(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor) : IBaseService<TModel> where TModel : class
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;
    protected readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Gets all entities with optional filtering, ordering, and pagination.
    /// </summary>
    /// <typeparam name="TDto">The type of DTO to map the entities to.</typeparam>
    /// <param name="asNoTracking">Indicates whether to use tracking or not.</param>
    /// <param name="resourceParams">Resource parameters for filtering, ordering, and pagination.</param>
    /// <param name="ignorePaging">Indicates whether to ignore pagination.</param>
    /// <param name="selector">Expression to select DTO properties.</param>
    /// <param name="columnName">The column name to use for ordering and filtering.</param>
    /// <returns>A collection of DTOs.</returns>
    public async Task<IEnumerable<TDto>> GetAllAsync<TDto>(bool asNoTracking = true, ResourceParams? resourceParams = null, bool ignorePaging = false, Expression<Func<TModel, TDto>>? selector = null, string columnName = "Id", bool asSplitQuery = false) where TDto : new()
    {
        var queryable = _unitOfWork.Repository<TModel>().GetQueryable<TModel>();

        // AsNoTracking
        if (asNoTracking)
            queryable = queryable.AsNoTracking();

        if (asSplitQuery)
            queryable = queryable.AsSplitQuery();

        // Filtering
        if (resourceParams?.FilterQuery != null)
            queryable = queryable.Where(resourceParams.FilterQuery);
        var orderByColumn = string.IsNullOrEmpty(resourceParams?.OrderBy) ? columnName : resourceParams.OrderBy;

        // Ordering
        var orderByExpression = GetOrderExpression(orderByColumn) ?? GetDefaultOrderExpression(columnName);
        queryable = resourceParams?.Asc ?? true ? queryable.OrderBy(orderByExpression) : queryable.OrderByDescending(orderByExpression);

        // Filtering by User and Role
        var filteredQuery = await FilterByUserAndRoleAsync(queryable);

        // Selecting columns
        var result = selector == null ? SelectDefaultColumns<TDto>(filteredQuery) : filteredQuery.Select(selector);

        // Pagination
        return await GetPagedResult(result, resourceParams, ignorePaging);
    }

    /// <summary>
    /// Gets an entity by ID with optional tracking, filtering by user and role.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="asNoTracking">Indicates whether to use tracking or not.</param>
    /// <param name="columnName">The column name to use for filtering.</param>
    /// <returns>The entity with the specified ID.</returns>
    public async Task<TModel?> GetByIdAsync(int id, bool asNoTracking = true, string columnName = "Id", List<string>? includedProperties = null, bool asSplitQuery = false)
    {
        var queryable = _unitOfWork.Repository<TModel>().GetQueryable<TModel>();
        if (includedProperties?.Count > 0)
            foreach (var includedProperty in includedProperties)
                queryable = queryable.Include(includedProperty);

        // Apply AsNoTracking if specified
        if (asNoTracking)
            queryable = queryable.AsNoTracking();

        if (asSplitQuery)
            queryable = queryable.AsSplitQuery();

        queryable = await FilterByUserAndRoleAsync(queryable);

        queryable = queryable.Where($"{columnName} = @0", id);

        var entity = await queryable.FirstOrDefaultAsync();
        return entity;
    }

    /// <summary>
    /// Gets an entity by ID with optional tracking, filtering by user and role, and projects to a DTO with optional column selection.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to project the entity into.</typeparam>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="asNoTracking">Indicates whether to use tracking or not.</param>
    /// <param name="columnName">The column name to use for filtering.</param>
    /// <param name="selector">The expression to select specific columns for the DTO.</param>
    /// <returns>The DTO with the specified columns of the entity with the specified ID.</returns>
    public async Task<TDto?> GetByIdAsync<TDto>(int id, bool asNoTracking = true, string columnName = "Id", Expression<Func<TModel, TDto>>? selector = null, bool asSplitQuery = false) where TDto : new()
    {
        var queryable = _unitOfWork.Repository<TModel>().GetQueryable<TModel>();

        // Apply AsNoTracking if needed
        if (asNoTracking)
            queryable = queryable.AsNoTracking();

        if (asSplitQuery)
            queryable = queryable.AsSplitQuery();

        queryable = await FilterByUserAndRoleAsync(queryable);

        queryable = queryable.Where(x => EF.Property<int>(x, columnName) == id);
        IQueryable<TDto> result;
        if (selector == null)
            result = queryable.Select(AutoMapProperties<TDto>());
        else
            result = queryable.Select(selector);

        var entity = await result.FirstOrDefaultAsync();
        return entity;
    }

    private static Expression<Func<TModel, TDto>> AutoMapProperties<TDto>() where TDto : new()
    {
        var modelParam = Expression.Parameter(typeof(TModel), "model");

        var bindings = typeof(TDto).GetProperties()
            .Select(dtoProp =>
            {
                var modelProp = typeof(TModel).GetProperty(dtoProp.Name);
                if (modelProp != null)
                {
                    return Expression.Bind(dtoProp, Expression.Property(modelParam, modelProp.Name));
                }
                return null;
            })
            .Where(binding => binding != null)
            .ToArray();

        return Expression.Lambda<Func<TModel, TDto>>(
            Expression.MemberInit(Expression.New(typeof(TDto)), bindings),
            modelParam
        );
    }


    /// <summary>
    /// Creates a new entity and returns its ID.
    /// </summary>
    /// <param name="model">The entity to create.</param>
    /// <param name="idColumnName">The column name to use for retrieving the ID.</param>
    /// <returns>The ID of the created entity.</returns>
    public async Task<int?> CreateAsync(TModel model, string idColumnName = "Id")
    {
        var entity = await _unitOfWork.Repository<TModel>().AddAsync(model);
        await _unitOfWork.CompleteAsync();

        var idProperty = typeof(TModel).GetProperty(idColumnName);
        var id = (int?)idProperty!.GetValue(entity);

        return id;
    }

    /// <summary>
    /// Updates an entity with the specified ID using either a direct update or a partial update.
    /// </summary>
    /// <param name="id">The ID of the entity to update.</param>
    /// <param name="model">The updated entity data.</param>
    /// <param name="usePartialUpdate">flag that indicates whether a partial update should be performed. A partial update involves updating only specific properties of the entity</param>
    /// <param name="idColumnName">The column name to use for the ID.</param>
    /// <param name="directUpdate">flag that indicates whether to perform a direct update. In a direct update, the entire entity is updated without considering specific properties.</param>
    /// <param name="updateType">Type specifying the properties to update in case of a partial update.</param>
    /// <returns>True if the update is successful; otherwise, false.</returns>
    public async Task<bool> UpdateAsync(int id, TModel model, bool usePartialUpdate = true, string idColumnName = "Id", bool directUpdate = false, Type? updateType = null, bool asSplitQuery = false)
    {
        if (directUpdate || !usePartialUpdate)
        {
            if (!directUpdate && !await AnyAsync(idColumnName, id))
                return false;

            var idProperty = typeof(TModel).GetProperty(idColumnName);
            idProperty?.SetValue(model, id);

            await _unitOfWork.Repository<TModel>().UpdateAsync(model);
            await _unitOfWork.CompleteAsync();
            return true;
        }
        else
        {
            var includedProperties = updateType?.GetProperties()
                .Where(prop =>
                    (prop.PropertyType.IsGenericType &&
                     typeof(ICollection<>).IsAssignableFrom(prop.PropertyType.GetGenericTypeDefinition())) ||
                    prop.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
                )
                .Select(prop => prop.Name)
                .ToList();

            var entity = await GetByIdAsync(id, false, idColumnName, includedProperties, asSplitQuery: asSplitQuery);
            if (entity == null) return false;

            var modelPatch = new JsonPatchDocument<TModel>();

            var properties = typeof(TModel).GetProperties();
            var updateProperties = updateType != null
                ? updateType.GetProperties().Select(p => p.Name)
                : properties.Select(p => p.Name);


            foreach (var property in properties)
            {
                if (property.Name == idColumnName || property.Name == "UserId")
                    continue;

                if (!updateProperties.Contains(property.Name))
                    continue;

                var oldValue = property.GetValue(entity);
                var newValue = property.GetValue(model);

                if (oldValue == null && newValue != null || (oldValue != null && !Equals(oldValue, newValue)))
                {
                    var parameter = Expression.Parameter(typeof(TModel), "prop");
                    var body = Expression.Property(parameter, property.Name);
                    var conversion = Expression.Convert(body, typeof(object));
                    var lambda = Expression.Lambda<Func<TModel, object>>(conversion, parameter);

                    modelPatch.Replace(lambda, newValue);
                }
            }

            modelPatch.ApplyTo(entity);
            await _unitOfWork.Repository<TModel>().ReplaceAsync(entity);
            await _unitOfWork.Repository<TModel>().CompleteAsync();
            return true;
        }
    }


    /// <summary>
    /// Checks if any entity in the repository matches the specified condition.
    /// </summary>
    /// <param name="columnName">The name of the column to check against.</param>
    /// <param name="value">The value to check for.</param>
    /// <returns>True if any entity matches the condition; otherwise, false.</returns>
    public async Task<bool> AnyAsync(string columnName, object value)
    {
        var parameter = Expression.Parameter(typeof(TModel), "x");
        var property = Expression.Property(parameter, columnName);
        var constant = Expression.Constant(value);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<TModel, bool>>(equality, parameter);

        return await _unitOfWork.Repository<TModel>().AnyAsync(lambda);
    }

    /// <summary>
    /// Deletes an entity with the specified ID from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>True if the deletion is successful; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id, false);
        if (entity == null)
            return false;

        await _unitOfWork.Repository<TModel>().HardDeleteAsync(entity);
        var result = await _unitOfWork.CompleteAsync();
        if (result == 0)
            return false;
        return true;
    }

    public async Task<bool> DeleteRangeAsync(List<TModel> entities)
    {
        await _unitOfWork.Repository<TModel>().HardDeleteRangeAsync(entities);
        var result = await _unitOfWork.CompleteAsync();
        if (result == 0)
            return false;
        return true;
    }

    /// <summary>
    /// Gets an order expression for the specified property to be used in sorting.
    /// </summary>
    /// <param name="orderBy">The property name to order by.</param>
    /// <returns>An order expression or null if the property is not found.</returns>
    private static Expression<Func<TModel, object>>? GetOrderExpression(string orderBy)
    {
        var property = typeof(TModel).GetProperty(orderBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
            return null;

        var parameter = Expression.Parameter(typeof(TModel));
        var propertyAccess = Expression.Property(parameter, property);
        var expression = Expression.Lambda<Func<TModel, object>>(Expression.Convert(propertyAccess, typeof(object)), parameter);

        return expression;
    }

    /// <summary>
    /// Filters the given queryable based on user and role permissions for the specified entity type.
    /// </summary>
    /// <param name="queryable">The queryable to filter.</param>
    /// <param name="role">The role for which to filter data.</param>
    /// <param name="userId">The user ID for whom to filter data.</param>
    /// <param name="Id">Optional ID parameter for additional filtering.</param>
    /// <returns>The filtered queryable based on user and role permissions.</returns>
    private async Task<IQueryable<TModel>> FilterByUserAndRoleAsync(IQueryable<TModel> queryable, Roles? role, string userId, int? Id = null)
    {
        var serviceMap = new Dictionary<Type, Func<IQueryable<object>, Roles?, string, int?, Task<IQueryable<object>>>>
            {
               
               };

        var type = typeof(TModel);

        if (serviceMap.TryGetValue(type, out var value))
        {
            var filteredQuery = await value(queryable.Cast<object>(), role, userId, Id);
            queryable = filteredQuery.Cast<TModel>();
        }

        return queryable;
    }

    public async Task<bool> PartialUpdateAsync(int id, TModel model, string idColumnName = "Id", Type? updateType = null)
    {
        var includedProperties = updateType?.GetProperties()
             .Where(prop =>
                 (prop.PropertyType.IsGenericType &&
                  typeof(ICollection<>).IsAssignableFrom(prop.PropertyType.GetGenericTypeDefinition())) ||
                 prop.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))
             )
             .Select(prop => prop.Name)
             .ToList();

        var queryable = _unitOfWork.Repository<TModel>().GetQueryable<TModel>();
        if (includedProperties?.Count > 0)
            foreach (var includedProperty in includedProperties)
                queryable = queryable.Include(includedProperty);

        var entity = await queryable.FirstOrDefaultAsync(e => EF.Property<int>(e, idColumnName) == id);
        if (entity == null)
            return false;

        var modelPatch = new JsonPatchDocument<TModel>();
        var properties = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var updateProperties = updateType != null
            ? updateType.GetProperties().Select(p => p.Name).ToHashSet()
            : properties.Select(p => p.Name).ToHashSet();

        var excludeProperties = new[] { idColumnName, "UserId" };

        foreach (var property in properties)
        {
            if (excludeProperties.Contains(property.Name) || !updateProperties.Contains(property.Name))
                continue;

            var oldValue = property.GetValue(entity);
            var newValue = property.GetValue(model);

            if ((oldValue == null && newValue != null) || (oldValue != null && !oldValue.Equals(newValue)))
            {
                var parameter = Expression.Parameter(typeof(TModel), "prop");
                var body = Expression.Property(parameter, property.Name);
                var conversion = Expression.Convert(body, typeof(object));
                var lambda = Expression.Lambda<Func<TModel, object?>>(conversion, parameter);
                modelPatch.Replace(lambda, newValue);
            }
        }
        modelPatch.ApplyTo(entity);
        try
        {
            await _unitOfWork.Repository<TModel>().CompleteAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static Expression<Func<TModel, object>> GetDefaultOrderExpression(string columnName)
    {
        var idProperty = typeof(TModel).GetProperty(columnName);
        if (idProperty == null)
            throw new ArgumentException($"Property '{columnName}' not found on type '{typeof(TModel).Name}'");

        var parameter = Expression.Parameter(typeof(TModel), "x");
        var propertyAccess = Expression.Property(parameter, idProperty);
        return Expression.Lambda<Func<TModel, object>>(Expression.Convert(propertyAccess, typeof(object)), parameter);
    }

    private async Task<IQueryable<TModel>> FilterByUserAndRoleAsync(IQueryable<TModel> queryable)
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var idClaim = _httpContextAccessor?.HttpContext?.User.FindFirst("Id")?.Value;

        if (_httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value is string roleValue &&
            Enum.TryParse(typeof(Roles), roleValue, out var userRole))
        {
            dynamic? parsedRole = userRole;
            dynamic? parsedId = int.TryParse(idClaim, out var id) ? id : null;
            return await FilterByUserAndRoleAsync(queryable, parsedRole, userId, parsedId);
        }

        return queryable;
    }

    private IQueryable<TDto> SelectDefaultColumns<TDto>(IQueryable<TModel> queryable) where TDto : new()
    {
        var dtoProperties = typeof(TDto).GetProperties();
        var modelParam = Expression.Parameter(typeof(TModel), "model");

        var bindings = typeof(TModel).GetProperties()
            .Where(mp => dtoProperties.Any(dp => dp.Name == mp.Name))
            .Select(modelProperty =>
            {
                var dtoProperty = dtoProperties.First(dp => dp.Name == modelProperty.Name);
                return Expression.Bind(dtoProperty, Expression.PropertyOrField(modelParam, modelProperty.Name));
            })
            .ToArray();

        var memberInit = Expression.MemberInit(Expression.New(typeof(TDto)), bindings);
        var selector = Expression.Lambda<Func<TModel, TDto>>(memberInit, modelParam);
        return queryable.Select(selector);
    }

    private async Task<IEnumerable<TDto>> GetPagedResult<TDto>(IQueryable<TDto> query, ResourceParams? resourceParams, bool ignorePaging)
    {
        if (ignorePaging)
            return await PagedList<TDto>.CreateAsync(query, 1, 5000);

        if (resourceParams == null)
            throw new ArgumentNullException(nameof(resourceParams), "Resource parameters are required for paging.");

        var pagedList = await PagedList<TDto>.CreateAsync(query, resourceParams.PageNumber, resourceParams.PageSize);
        if (pagedList == null)
            return new PagedList<TDto>([], 0, 0, 1);
        else return pagedList;
    }
}
