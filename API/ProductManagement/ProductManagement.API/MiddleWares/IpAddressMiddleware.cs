using ProductManagement.API.Helpers;

namespace ProductManagement.API.MiddleWares;

public class IpAddressMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IpAddressMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
    {
        _next = next;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Invoke(HttpContext context)
    {
        var ipAddress = IPHelper.GetIPAddress(_httpContextAccessor.HttpContext!);
        context.Items["IpAddress"] = ipAddress;
        await _next(context);
    }
}
