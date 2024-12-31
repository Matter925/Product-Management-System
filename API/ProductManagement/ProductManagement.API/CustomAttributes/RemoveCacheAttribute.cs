using Microsoft.AspNetCore.Mvc.Filters;

using ProductManagement.Services.Interfaces;

namespace ProductManagement.API.CustomAttributes;

[AttributeUsage(AttributeTargets.Method)]
public class RemoveCacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _CacheKeyName = null!;

    public RemoveCacheAttribute(string cacheKeyName)
    {
        _CacheKeyName = cacheKeyName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
        var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
        cacheService.RemoveCachedResponse(_CacheKeyName);
    }
}
