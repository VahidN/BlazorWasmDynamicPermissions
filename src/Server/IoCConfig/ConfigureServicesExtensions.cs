using System.Text;
using BlazorWasmDynamicPermissions.Server.DataLayer.Context;
using BlazorWasmDynamicPermissions.Server.Models.SiteOptions;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlazorWasmDynamicPermissions.Server.IoCConfig;

public static class ConfigureServicesExtensions
{
    public static void AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .WithOrigins(
                        "http://localhost:4200") //Note:  The URL must be specified without a trailing slash (/).
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(host => true)
                    .AllowCredentials());
        });
    }

    public static void AddCustomJwtBearer(this IServiceCollection services, ConfigurationManager configuration)
    {
        // Only needed for custom roles.
        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomRoles.Admin, policy => policy.RequireRole(CustomRoles.Admin));
            options.AddPolicy(CustomRoles.User, policy => policy.RequireRole(CustomRoles.User));
            options.AddPolicy(CustomRoles.Editor, policy => policy.RequireRole(CustomRoles.Editor));
        });

        // Needed for jwt auth.
        services
            .AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                var bearerTokenOption = configuration.Get<SiteSettingsDto>()?.BearerToken;
                if (bearerTokenOption is null)
                {
                    throw new InvalidOperationException("siteSettings.BearerToken is null");
                }

                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = bearerTokenOption.Issuer, // site that makes the token
                    ValidateIssuer = true,
                    ValidAudience = bearerTokenOption.Audience, // site that consumes the token
                    ValidateAudience = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenOption.Key)),
                    ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                    ValidateLifetime = true, // validate the expiration
                    ClockSkew = TimeSpan.Zero // tolerance for the expiration date
                };
                cfg.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var tokenValidatorService =
                            context.HttpContext.RequestServices.GetRequiredService<ITokenValidatorService>();
                        return tokenValidatorService.ValidateAccessTokenAsync(context);
                    }
                };
            });
    }

    public static void AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services.AddDbContextPool<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
        {
            var connectionString = GetConnectionString(configuration, serviceProvider);
            optionsBuilder.UseSqlite(connectionString,
                sqliteOptionsBuilder =>
                {
                    var minutes = (int)TimeSpan.FromMinutes(3).TotalSeconds;
                    sqliteOptionsBuilder.CommandTimeout(minutes);
                    sqliteOptionsBuilder.MigrationsAssembly(typeof(ConfigureServicesExtensions).Assembly.FullName);
                });
        });
    }

    private static string? GetConnectionString(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var webHostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
        var rootPath = webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            rootPath = AppContext.BaseDirectory;
        }

        var dataDir = Path.Combine(rootPath, "wwwroot", "App_Data", "Database");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection")?
            .Replace("|DataDirectory|", dataDir, StringComparison.OrdinalIgnoreCase);
        return connectionString;
    }

    public static void AddCustomServices(this IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IApiActionsDiscoveryService, ApiActionsDiscoveryService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IUserTokensService, UserTokensService>();
        services.AddScoped<IUserClaimsService, UserClaimsService>();
        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddScoped<IDbInitializerService, DbInitializerService>();
        services.AddScoped<ITokenValidatorService, TokenValidatorService>();
        services.AddScoped<ITokenFactoryService, TokenFactoryService>();

        services.AddDynamicPermissions();
    }

    private static IServiceCollection AddDynamicPermissions(this IServiceCollection services)
    {
        services.AddScoped<IServerSecurityTrimmingService, ServerSecurityTrimmingService>();
        services.AddScoped<IAuthorizationHandler, DynamicServerPermissionsAuthorizationHandler>();
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy(
                CustomPolicies.DynamicServerPermission,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new DynamicServerPermissionRequirement());
                });
        });

        return services;
    }

    public static void AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SiteSettingsDto>(configuration.Bind);
    }
}