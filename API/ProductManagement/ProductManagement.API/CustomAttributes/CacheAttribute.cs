using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using ProductManagement.Services.Interfaces;

namespace ProductManagement.API.CustomAttributes;

[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _TimeToLiveInHours;
    private readonly string _CacheKeyName = null!;

    public CacheAttribute(string cacheKeyName, int timeToLiveInHours = 24)
    {
        _TimeToLiveInHours = timeToLiveInHours;
        _CacheKeyName = cacheKeyName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
        var response = cacheService.GetCachedResponse(_CacheKeyName);

        if (!string.IsNullOrEmpty(response))
        {
            var contentResult = new ContentResult
            {
                Content = response,
                ContentType = "application/json",
                StatusCode = 200,
            };

            context.Result = contentResult;
            return;
        }

        var executedContent = await next();
        if (executedContent.Result is OkObjectResult okObjectResult)
            cacheService.SetCacheResponse(_CacheKeyName, okObjectResult.Value, TimeSpan.FromHours(_TimeToLiveInHours));
    }
}
