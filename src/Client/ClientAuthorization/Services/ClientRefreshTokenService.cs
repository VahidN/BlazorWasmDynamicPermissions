using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class ClientRefreshTokenService : IClientRefreshTokenService
{
    private readonly IBearerTokensStore _bearerTokensStore;
    private readonly IHttpClientService _httpClientService;
    private readonly ILogger<ClientRefreshTokenService> _logger;
    private readonly NavigationManager _navigationManager;

    public ClientRefreshTokenService(
        IHttpClientService httpClientService,
        IBearerTokensStore bearerTokensStore,
        NavigationManager navigationManager,
        ILogger<ClientRefreshTokenService> logger)
    {
        _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
        _bearerTokensStore = bearerTokensStore ?? throw new ArgumentNullException(nameof(bearerTokensStore));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Server-side validation url
    /// </summary>
    public string? ValidateAccessTokenUrl { set; get; } = "api/UsersAccountManager/ValidateAccessToken";

    /// <summary>
    ///     The server-side refresh token creator's url
    /// </summary>
    public string? RefreshTokenUrl { set; get; } = "api/UsersAccountManager/RefreshToken";

    /// <summary>
    ///     Client-side and server-side validation of the current token
    /// </summary>
    public async Task ValidateAndRefreshTokenOnErrorsAsync(HttpRequestMessage? request)
    {
        if (await IsAccessTokenStillValidAsync())
        {
            _logger.LogInformation("Skipping the refresh token. Current access token is still valid.");
            return;
        }

        var tokens = await TryRefreshTokenAsync();
        if (tokens is null)
        {
            _logger.LogInformation(
                "Failed to refresh the token and current access token is invalid . Logging out the user.");
            _navigationManager.NavigateTo("logout");
            return;
        }

        SetAccessTokenHeader(request, tokens.AccessToken);
    }

    public async Task<UserTokensDto?> TryRefreshTokenAsync()
    {
        _logger.LogInformation("Trying to refresh the token.");

        if (string.IsNullOrWhiteSpace(RefreshTokenUrl))
        {
            throw new InvalidOperationException("Please specify the `RefreshTokenUrl` value.");
        }

        var tokenInfo = await _bearerTokensStore.GetBearerTokenAsync(BearerTokenType.RefreshToken);
        if (tokenInfo is null)
        {
            _logger.LogInformation("There is no valid refresh token to use it.");
            return null;
        }

        var response = await _httpClientService.PostDataAsJsonAsync<UserTokensDto?>(
            RefreshTokenUrl, new UserTokensDto { RefreshToken = tokenInfo.Token });
        if (response is null)
        {
            await _bearerTokensStore.RemoveAllBearerTokensAsync();
            return null;
        }

        await _bearerTokensStore.StoreAllTokensAsync(response);
        return response;
    }

    public void SetAccessTokenHeader(HttpRequestMessage? request, string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || request is null)
        {
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
    }

    public async Task<bool> IsAccessTokenStillValidAsync()
    {
        _logger.LogInformation("Doing client-side and server-side validation of the current access token.");

        if (string.IsNullOrWhiteSpace(ValidateAccessTokenUrl))
        {
            throw new InvalidOperationException("Please specify the `ValidateTokenUrl` value.");
        }

        var tokenInfo = await GetCurrentAccessTokenAsync();
        if (tokenInfo is null)
        {
            _logger.LogInformation("Client-side validation of the current access token failed.");
            return false;
        }

        var response = await _httpClientService.PostDataAsJsonAsync<ApiResponseDto>(
            ValidateAccessTokenUrl, new UserTokensDto { AccessToken = tokenInfo.Token });
        var isSucceeded = response?.Success == true;
        if (!isSucceeded)
        {
            _logger.LogInformation("Server-side validation of the current access token failed.");
            await _bearerTokensStore.RemoveBearerTokenAsync(BearerTokenType.AccessToken);
        }

        return isSucceeded;
    }

    /// <summary>
    ///     Client-side and server-side validation of the current token
    /// </summary>
    public async Task<BearerTokenInfo?> ValidateAndRefreshTokenOnStartupAsync()
    {
        _logger.LogInformation("Validate And RefreshToken OnStartup.");

        if (await IsAccessTokenStillValidAsync())
        {
            _logger.LogInformation("Skipping the refresh token. Current access token is still valid.");
            return await GetCurrentAccessTokenAsync();
        }

        var tokens = await TryRefreshTokenAsync();
        if (tokens is null)
        {
            _logger.LogInformation("Failed to refresh the token.");
            return null;
        }

        return await GetCurrentAccessTokenAsync();
    }

    private Task<BearerTokenInfo?> GetCurrentAccessTokenAsync()
    {
        return _bearerTokensStore.GetBearerTokenAsync(BearerTokenType.AccessToken);
    }
}