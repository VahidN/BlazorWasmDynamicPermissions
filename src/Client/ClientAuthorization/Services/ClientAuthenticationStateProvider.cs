using System.Security.Claims;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

/// <summary>
///     A custom AuthenticationStateProvider.
///     Provides information about the authentication state of the current user.
///     It should be added as builder.Services.AddScoped -> AuthenticationStateProvider, ClientAuthenticationStateProvider
///     After calling the builder.Services.AddAuthorizationCore(options => options.AddAppPolicies());
/// </summary>
public class ClientAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IBearerTokensStore _bearerTokensStore;
    private readonly ILogger<ClientAuthenticationStateProvider> _logger;
    private readonly IClientRefreshTokenService _refreshTokenService;
    private readonly IClientRefreshTokenTimer _refreshTokenTimer;

    /// <summary>
    ///     A custom AuthenticationStateProvider.
    ///     Provides information about the authentication state of the current user.
    /// </summary>
    public ClientAuthenticationStateProvider(
        IClientRefreshTokenService refreshTokenService,
        IBearerTokensStore bearerTokensStore,
        IClientRefreshTokenTimer refreshTokenTimer,
        ILogger<ClientAuthenticationStateProvider> logger)
    {
        _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
        _bearerTokensStore = bearerTokensStore ?? throw new ArgumentNullException(nameof(bearerTokensStore));
        _refreshTokenTimer = refreshTokenTimer ?? throw new ArgumentNullException(nameof(refreshTokenTimer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Describes the current user
    /// </summary>
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _logger.LogInformation("Get authentication state.");
        return GetCurrentAuthenticationStateAsync(true);
    }

    /// <summary>
    ///     Calls NotifyAuthenticationStateChanged about the new jwtInfo.Claims
    /// </summary>
    public async Task NotifyUserLoggedInAsync()
    {
        var authState = Task.FromResult(await GetCurrentAuthenticationStateAsync(false));
        NotifyAuthenticationStateChanged(authState);
    }

    /// <summary>
    ///     Calls NotifyAuthenticationStateChanged about the new jwtInfo.Claims
    /// </summary>
    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(GetEmptyAuthenticationState());
        NotifyAuthenticationStateChanged(authState);
    }

    private async Task<AuthenticationState> GetCurrentAuthenticationStateAsync(bool validateTokens)
    {
        BearerTokenInfo? jwtInfo;
        if (validateTokens)
        {
            jwtInfo = await _refreshTokenService.ValidateAndRefreshTokenOnStartupAsync();
            if (jwtInfo is not null)
            {
                await _refreshTokenTimer.StartRefreshTimerAsync();
            }
        }
        else
        {
            jwtInfo = await _bearerTokensStore.GetBearerTokenAsync(BearerTokenType.AccessToken);
        }

        if (jwtInfo is null)
        {
            return GetEmptyAuthenticationState();
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(jwtInfo.Claims, "jwtAuthType")));
    }

    private static AuthenticationState GetEmptyAuthenticationState()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}