using Blazored.LocalStorage;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class BearerTokensStore : IBearerTokensStore
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<BearerTokensStore> _logger;

    public BearerTokensStore(
        ILocalStorageService localStorage,
        ILogger<BearerTokensStore> logger)
    {
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BearerTokenInfo?> GetBearerTokenAsync(BearerTokenType tokenType)
    {
        var token = await _localStorage.GetItemAsync<string>(tokenType.ToString());
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var jwtInfo = BearerTokenParser.ParseClaimsFromJwt(token);
        if (jwtInfo.IsExpired)
        {
            await RemoveBearerTokenAsync(tokenType);
            _logger.LogInformation("`{TokenType}` was expired and removed.", tokenType);

            return null;
        }

        return jwtInfo;
    }

    /// <summary>
    ///     Stores the accessToken in the localStorage
    /// </summary>
    public async Task StoreBearerTokenAsync(string? token, BearerTokenType tokenType)
    {
        if (token is null)
        {
            return;
        }

        await _localStorage.SetItemAsync(tokenType.ToString(), token);
    }

    /// <summary>
    ///     Removes the accessToken from the localStorage
    /// </summary>
    public async Task RemoveBearerTokenAsync(BearerTokenType tokenType)
    {
        await _localStorage.RemoveItemAsync(tokenType.ToString());
    }

    /// <summary>
    ///     Removes the accessToken, refresh token and dynamic permissions token from the localStorage
    /// </summary>
    public async Task RemoveAllBearerTokensAsync()
    {
        await _localStorage.RemoveItemAsync(BearerTokenType.AccessToken.ToString());
        await _localStorage.RemoveItemAsync(BearerTokenType.RefreshToken.ToString());
        await _localStorage.RemoveItemAsync(BearerTokenType.DynamicPermissions.ToString());
    }

    /// <summary>
    ///     Adds the accessToken, refresh token and dynamic permissions token to the localStorage
    /// </summary>
    public async Task StoreAllTokensAsync(UserTokensDto? response)
    {
        await StoreBearerTokenAsync(response?.DynamicPermissionsToken, BearerTokenType.DynamicPermissions);
        await StoreBearerTokenAsync(response?.AccessToken, BearerTokenType.AccessToken);
        await StoreBearerTokenAsync(response?.RefreshToken, BearerTokenType.RefreshToken);
    }
}