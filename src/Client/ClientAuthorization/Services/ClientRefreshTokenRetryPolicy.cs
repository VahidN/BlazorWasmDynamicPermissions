using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using Polly;
using Polly.Retry;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public static class ClientRefreshTokenRetryPolicy
{
    public static AsyncRetryPolicy<HttpResponseMessage> RefreshToken(
        IServiceProvider serviceProvider, HttpRequestMessage request)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(ex =>
                ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            .Or<HttpRequestException>(ex =>
                ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            .WaitAndRetryAsync(
                new[] { TimeSpan.FromSeconds(1) },
                async (response, delay, retryCount, context) =>
                {
#if DEBUG
                    WriteLine(
                        $"Running the `Client RefreshToken Retry Policy` for StatusCode: {response.Result.StatusCode} & Exception: {response.Exception}.");
#endif
                    var refreshTokenService = serviceProvider.GetRequiredService<IClientRefreshTokenService>();
                    await refreshTokenService.ValidateAndRefreshTokenOnErrorsAsync(request);
                });
    }
}