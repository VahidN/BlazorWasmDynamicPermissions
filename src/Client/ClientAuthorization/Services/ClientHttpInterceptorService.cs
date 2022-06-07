using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

/// <summary>
///     HttpClient Interceptor
/// </summary>
public class ClientHttpInterceptorService : DelegatingHandler
{
    private readonly IBearerTokensStore _bearerTokensStore;

    /// <summary>
    ///     HttpClient Interceptor
    /// </summary>
    public ClientHttpInterceptorService(IBearerTokensStore bearerTokensStore)
    {
        _bearerTokensStore = bearerTokensStore ?? throw new ArgumentNullException(nameof(bearerTokensStore));
    }

    /// <summary>
    ///     Sends an HTTP request to the inner handler to send to the server
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        await AddAccessTokenToAllRequestsAsync(request);

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }

    private async Task AddAccessTokenToAllRequestsAsync(HttpRequestMessage request)
    {
        var tokenInfo = await _bearerTokensStore.GetBearerTokenAsync(BearerTokenType.AccessToken);
        request.Headers.Authorization =
            tokenInfo is not null ? new AuthenticationHeaderValue("bearer", tokenInfo.Token) : null;
    }
}