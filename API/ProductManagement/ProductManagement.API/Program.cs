using System.Security.Claims;
using System.Transactions;

using Hangfire;

using Microsoft.EntityFrameworkCore;


using ProductManagement.API.Extensions;
using ProductManagement.API.Helpers;
using ProductManagement.API.MiddleWares;
using ProductManagement.EFCore.IdentityModels;

var builder = WebApplication.CreateBuilder(args);
TransactionManager.ImplicitDistributedTransactions = true;

// Add services to the container.
builder.Services.AddHangfireServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger();
builder.Services.AddCustomDbContexts(builder.Configuration);
builder.Services.AddCustomAuthentication(builder.Configuration);

#region Rate Limiting
// Rate Limiting
//builder.Services.AddRateLimiter(options =>
//{
//    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
//        RateLimitPartition.GetTokenBucketLimiter(
//        partitionKey: httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? httpContext.Request.Headers.Host.ToString(),
//        factory: partition => new TokenBucketRateLimiterOptions
//        {
//            TokenLimit = 1000,
//            TokensPerPeriod = 500,
//            ReplenishmentPeriod = TimeSpan.FromSeconds(15)
//        }));

//    //options.AddTokenBucketLimiter("bucket", options =>
//    //{
//    //    options.ReplenishmentPeriod = TimeSpan.FromSeconds(15);
//    //    options.TokenLimit = 3;
//    //    options.TokensPerPeriod = 2;
//    //});

//    options.OnRejected = async (context, token) =>
//    {
//        context.HttpContext.Response.StatusCode = 429;
//        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
//        {
//            await context.HttpContext.Response.WriteAsync(
//                $"Too many requests. Please try again after {retryAfter.TotalSeconds} Seconds.");
//        }
//        else
//        {
//            await context.HttpContext.Response.WriteAsync(
//                "Too many requests. Please try again later.");
//        }
//    };
//});
#endregion

builder.Services.ConfigureApiBehavior();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddCustomServices();
builder.Services.AddCors();
builder.Services.AddAuthorization();

// Caching
builder.Services.AddCustomCaching(builder.Configuration);

builder.Services.AddHttpClient();
builder.Services.AddSignalR();

// Mini Profiler
builder.Services.AddCustomMiniProfiler(builder.Configuration);

var app = builder.Build();
app.UseMiniProfiler();
// Configure the HTTP request pipeline.
// TODO: uncomment the following line before publishing to production
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}
//if (app.Environment.IsDevelopment())
//{
app.UseStaticFiles();
////}
app.UseCors(c => c.WithOrigins(
    builder.Configuration["ClientFrontEndUrl"],
        builder.Configuration["AngularApp"],
        builder.Configuration["FrontEndUrl"],
        "https://ProductManagement-dashboard.runasp.net",
        "https://ProductManagement-app.vercel.app",
        "https://localhost:7255",
        "https://localhost:44360",
        "http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithExposedHeaders("*"));


app.UseMiddleware<IpAddressMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//app.UseRateLimiter();
app.MapControllers();
//app.CallAPIsOnStartup(builder.Configuration["ApiURL"]!);
app.UseOutputCache();
await app.RunAsync();