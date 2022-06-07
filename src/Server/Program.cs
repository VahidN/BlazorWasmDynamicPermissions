using BlazorWasmDynamicPermissions.Server.IoCConfig;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();
ConfigureMiddlewares(webApp, webApp.Environment);
ConfigureEndpoints(webApp);
ConfigureDatabase(webApp);
webApp.Run();

void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
{
    services.AddCustomConfiguration(configuration);
    services.AddCustomServices();
    services.AddCustomDbContext(configuration);
    services.AddCustomJwtBearer(configuration);
    services.AddCustomCors();
}

void ConfigureLogging(ILoggingBuilder logging, IHostEnvironment env, IConfiguration configuration)
{
    logging.ClearProviders();

    logging.AddDebug();

    if (env.IsDevelopment())
    {
        logging.AddConsole();
    }

    logging.AddConfiguration(configuration.GetSection("Logging"));
}

void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
{
    if (!env.IsDevelopment())
    {
        app.UseHsts();
    }
    else
    {
        app.UseWebAssemblyDebugging();
    }

    app.UseHttpsRedirection();

    app.UseStatusCodePages();

    app.UseBlazorFrameworkFiles();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();

    app.UseCors("CorsPolicy");

    app.UseAuthorization();
}

void ConfigureEndpoints(WebApplication app)
{
    app.MapRazorPages();
    app.MapControllers();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");
    });

    // catch-all handler for HTML5 client routes - serve index.html
    app.MapFallbackToFile("index.html");
}

void ConfigureDatabase(IApplicationBuilder app)
{
    var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializerService>();
    dbInitializer.Initialize();
    dbInitializer.SeedData();
}