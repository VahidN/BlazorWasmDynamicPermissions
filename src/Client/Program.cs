using Blazored.LocalStorage;
using BlazorWasmDynamicPermissions.Client;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Polly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ClientHttpInterceptorService>();
builder.Services.AddHttpClient(
        "ServerAPI",
        client =>
        {
            client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            client.DefaultRequestHeaders.Add("User-Agent", "DNTCommon.Client 1.0");
            client.Timeout = TimeSpan.FromMinutes(20);
        }
    )
    .AddHttpMessageHandler<ClientHttpInterceptorService>()
    // transient errors: network failures and HTTP 5xx/InternalServerError and HTTP 408/Timeout errors
    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(5) }))
    .AddPolicyHandler((serviceProvider, request) =>
        ClientRefreshTokenRetryPolicy.RefreshToken(serviceProvider, request));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));
builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>(); // Remove logging of the HttpClient

builder.Services.AddAuthorizationCore(options => options.AddClientPolicies());
builder.Services.AddScoped<AuthenticationStateProvider, ClientAuthenticationStateProvider>();

builder.Services.AddSingleton<IProtectedPagesProvider, ProtectedPagesProvider>();
builder.Services.AddScoped<IHttpClientService, HttpClientService>();
builder.Services.AddScoped<IDynamicClientPermissionsProvider, DynamicClientPermissionsProvider>();
builder.Services.AddScoped<IBearerTokensStore, BearerTokensStore>();
builder.Services.AddScoped<IClientRefreshTokenTimer, ClientRefreshTokenTimer>();
builder.Services.AddScoped<IClientRefreshTokenService, ClientRefreshTokenService>();
builder.Services.AddScoped<IClientSecurityTrimmingService, ClientSecurityTrimmingService>();
builder.Services.AddScoped<IAuthorizationHandler, DynamicClientPermissionsAuthorizationHandler>();

await builder.Build().RunAsync();