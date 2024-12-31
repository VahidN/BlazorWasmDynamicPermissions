using BlazorWasmDynamicPermissions.Server;
using BlazorWasmDynamicPermissions.Server.IoCConfig;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using _Imports = BlazorWasmDynamicPermissions.Client._Imports;

var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();
ConfigureMiddlewares(webApp, webApp.Environment);
ConfigureDatabase(webApp);
await webApp.RunAsync();

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

    logging.AddConfiguration(configuration.GetSection(key: "Logging"));
}

void ConfigureMiddlewares(WebApplication app, IHostEnvironment env)
{
    if (!env.IsDevelopment())
    {
        app.UseExceptionHandler(errorHandlingPath: "/Error", createScopeForErrors: true);

        app.UseHsts();
    }
    else
    {
        app.UseWebAssemblyDebugging();
    }

    app.UseHttpsRedirection();

    app.UseStatusCodePages();

    app.MapStaticAssets();
    app.UseAuthentication();
    app.UseCors(policyName: "CorsPolicy");
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers().WithStaticAssets();

    app.MapRazorComponents<App>()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(_Imports).Assembly)
        .AllowAnonymous();
}

void ConfigureDatabase(IApplicationBuilder app)
{
    var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializerService>();
    dbInitializer.Initialize();
    dbInitializer.SeedData();
}