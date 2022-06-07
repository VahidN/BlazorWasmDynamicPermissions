using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;

public interface IClientRefreshTokenService
{
    /// <summary>
    ///     Server-side validation url
    /// </summary>
    string? ValidateAccessTokenUrl { set; get; }

    /// <summary>
    ///     The server-side refresh token creator's url
    /// </summary>
    string? RefreshTokenUrl { set; get; }

    /// <summary>
    ///     Client-side and server-side validation of the current token
    /// </summary>
    Task ValidateAndRefreshTokenOnErrorsAsync(HttpRequestMessage request);

    Task<BearerTokenInfo?> ValidateAndRefreshTokenOnStartupAsync();

    void SetAccessTokenHeader(HttpRequestMessage? request, string? token);

    Task<UserTokensDto?> TryRefreshTokenAsync();

    Task<bool> IsAccessTokenStillValidAsync();
}