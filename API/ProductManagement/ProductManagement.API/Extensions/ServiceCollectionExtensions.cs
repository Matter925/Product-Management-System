using System.Text;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using ProductManagement.API.Helpers;
using ProductManagement.API.ResponseModule;
using ProductManagement.EFCore.Abstractions;
using ProductManagement.EFCore.Generic;
using ProductManagement.EFCore.IdentityModels;
using ProductManagement.EFCore.Models;
using ProductManagement.Services.Implementation;
using ProductManagement.Services.Interfaces;
using ProductManagement.Services.Implementation;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ILookupsService, LookupsService>();
        
        services.AddScoped<IOTPService, OTPService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddAutoMapper(typeof(MappingProfiles));
        services.AddScoped<IProductsService, ProductsService>();

        

        return services;
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        }).AddEntityFrameworkStores<ProductManagementDBContext>()
          .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true, 
                ValidateIssuerSigningKey = true, 
                ValidIssuer = configuration["Authentication:Issuer"], 
                ValidAudience = configuration["Authentication:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(configuration["Authentication:SecretForKey"]!)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/NotificationHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(setupAction =>
        {
            setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                Description = "Enter your bearer token in this format: Bearer {your-token}"
            });
            setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer" }
                    }, new List<string>() }
            });
        });

        return services;
    }

    public static void AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")!));
        services.AddHangfireServer();
    }

    public static void AddCustomDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductManagementDBContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")!);
        });

       
    }

    public static void AddCustomCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddOutputCache((opts) =>
        {
            var longLivingCacheHours = double.Parse(configuration["LongLivingOutputCacheHours"]!);
            var shortLivingCacheHours = double.Parse(configuration["ShortLivingOutputCacheHours"]!);

            opts.AddPolicy("Coupons", policy => policy.AddPolicy<MyCustomPolicy>().Expire(TimeSpan.FromHours(longLivingCacheHours)).Tag("Coupons"), true);
            opts.AddPolicy("Blogs", policy => policy.AddPolicy<MyCustomPolicy>().Expire(TimeSpan.FromHours(longLivingCacheHours)).Tag("Blogs"), true);
            opts.AddPolicy("Advertisements", policy => policy.AddPolicy<MyCustomPolicy>().Expire(TimeSpan.FromHours(longLivingCacheHours)).Tag("Advertisements"), true);
            opts.AddPolicy("LongLivingOutputCache", policy => policy.AddPolicy<MyCustomPolicy>().Expire(TimeSpan.FromHours(longLivingCacheHours)).Tag("tag-LongLivingOutputCache"), true);
            opts.AddPolicy("ShortLivingOutputCache", policy => policy.AddPolicy<MyCustomPolicy>().Expire(TimeSpan.FromHours(shortLivingCacheHours)).Tag("tag-ShortLivingOutputCache"), true);
        });
    }

    public static void AddCustomMiniProfiler(this IServiceCollection services, IConfiguration configuration)
    {
        //https://localhost:7143/2d9d7868-395e-478d-b72c-33b3e8f163a9/results  //MiniProfiler
        //https://localhost:7143/2d9d7868-395e-478d-b72c-33b3e8f163a9/results-index //MiniProfiler
        services.AddMiniProfiler(options =>
        {
            options.RouteBasePath = $"/{configuration["MiniProfiler"]}";
            (options.Storage as MemoryCacheStorage)!.CacheDuration = TimeSpan.FromSeconds(120);
            options.IgnoredPaths.Add("/swagger");
            options.ColorScheme = ColorScheme.Auto;
        }).AddEntityFramework();
    }

    public static void ConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var errorResponse = new ApiValidationErrorResponse { Errors = errors };
                return new BadRequestObjectResult(errorResponse);
            });
    }
}
