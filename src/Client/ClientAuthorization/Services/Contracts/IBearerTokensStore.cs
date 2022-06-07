using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;

public interface IBearerTokensStore
{
    Task<BearerTokenInfo?> GetBearerTokenAsync(BearerTokenType tokenType);

    /// <summary>
    ///     Stores the accessToken in the localStorage
    /// </summary>
    Task StoreBearerTokenAsync(string? token, BearerTokenType tokenType);

    /// <summary>
    ///     Adds the accessToken, refresh token and dynamic permissions token to the localStorage
    /// </summary>
    Task StoreAllTokensAsync(UserTokensDto? response);

    /// <summary>
    ///     Removes the accessToken from the localStorage
    /// </summary>
    Task RemoveBearerTokenAsync(BearerTokenType tokenType);

    /// <summary>
    ///     Removes the accessToken, refresh token and dynamic permissions token from the localStorage
    /// </summary>
    Task RemoveAllBearerTokensAsync();
}