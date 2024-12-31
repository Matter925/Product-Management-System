using Serilog.Ui.Web.Authorization;

namespace ProductManagement.API.Helpers;

public class ProductionAuthorizationFilter : IUiAuthorizationFilter, IUiAsyncAuthorizationFilter
{
    public bool Authorize(HttpContext httpContext)
    {
        return true;
    }

    public Task<bool> AuthorizeAsync(HttpContext httpContext)
    {
        return Task.FromResult(true);
    }
}
