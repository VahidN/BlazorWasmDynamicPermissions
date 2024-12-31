using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class ClientRefreshTokenService(
    IHttpClientService httpClientService,
    IBearerTokensStore bearerTokensStore,
    NavigationManager navigationManager,
    ILogger<ClientRefreshTokenService> logger) : IClientRefreshTokenService
{
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
            logger.LogInformation(message: "Skipping the refresh token. Current access token is still valid.");

            return;
        }

        var tokens = await TryRefreshTokenAsync();

        if (tokens is null)
        {
            logger.LogInformation(
                message: "Failed to refresh the token and current access token is invalid . Logging out the user.");

            navigationManager.NavigateTo(uri: "logout");

            return;
        }

        SetAccessTokenHeader(request, tokens.AccessToken);
    }

    public async Task<UserTokensDto?> TryRefreshTokenAsync()
    {
        try
        {
            logger.LogInformation(message: "Trying to refresh the token.");

            if (string.IsNullOrWhiteSpace(RefreshTokenUrl))
            {
                throw new InvalidOperationException(message: "Please specify the `RefreshTokenUrl` value.");
            }

            var tokenInfo = await bearerTokensStore.GetBearerTokenAsync(BearerTokenType.RefreshToken);

            if (tokenInfo is null)
            {
                logger.LogInformation(message: "There is no valid refresh token to use it.");

                return null;
            }

            var response = await httpClientService.PostDataAsJsonAsync<UserTokensDto?>(RefreshTokenUrl,
                new UserTokensDto
                {
                    RefreshToken = tokenInfo.Token
                });

            if (response is null)
            {
                await bearerTokensStore.RemoveAllBearerTokensAsync();

                return null;
            }

            await bearerTokensStore.StoreAllTokensAsync(response);

            return response;
        }
        catch (Exception ex)
        {
            await bearerTokensStore.RemoveAllBearerTokensAsync();
            logger.LogError(ex, message: "TryRefreshTokenAsync error");

            return null;
        }
    }

    public void SetAccessTokenHeader(HttpRequestMessage? request, string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || request is null)
        {
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(scheme: "bearer", token);
    }

    public async Task<bool> IsAccessTokenStillValidAsync()
    {
        logger.LogInformation(message: "Doing client-side and server-side validation of the current access token.");

        if (string.IsNullOrWhiteSpace(ValidateAccessTokenUrl))
        {
            throw new InvalidOperationException(message: "Please specify the `ValidateTokenUrl` value.");
        }

        var tokenInfo = await GetCurrentAccessTokenAsync();

        if (tokenInfo is null)
        {
            logger.LogInformation(message: "Client-side validation of the current access token failed.");

            return false;
        }

        var response = await httpClientService.PostDataAsJsonAsync<ApiResponseDto>(ValidateAccessTokenUrl,
            new UserTokensDto
            {
                AccessToken = tokenInfo.Token
            });

        var isSucceeded = response?.Success == true;

        if (!isSucceeded)
        {
            logger.LogInformation(message: "Server-side validation of the current access token failed.");
            await bearerTokensStore.RemoveBearerTokenAsync(BearerTokenType.AccessToken);
        }

        return isSucceeded;
    }

    /// <summary>
    ///     Client-side and server-side validation of the current token
    /// </summary>
    public async Task<BearerTokenInfo?> ValidateAndRefreshTokenOnStartupAsync()
    {
        logger.LogInformation(message: "Validate And RefreshToken OnStartup.");

        if (await IsAccessTokenStillValidAsync())
        {
            logger.LogInformation(message: "Skipping the refresh token. Current access token is still valid.");

            return await GetCurrentAccessTokenAsync();
        }

        var tokens = await TryRefreshTokenAsync();

        if (tokens is null)
        {
            logger.LogInformation(message: "Failed to refresh the token.");

            return null;
        }

        return await GetCurrentAccessTokenAsync();
    }

    private Task<BearerTokenInfo?> GetCurrentAccessTokenAsync()
        => bearerTokensStore.GetBearerTokenAsync(BearerTokenType.AccessToken);
}