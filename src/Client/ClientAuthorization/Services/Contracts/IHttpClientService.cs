namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;

/// <summary>
///     A custom  HttpClient Service
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    ///     Posts data as json to the endpoint
    /// </summary>
    Task<TResult?> PostDataAsJsonAsync<TResult>(string requestUri, object? value = null);

    /// <summary>
    ///     Posts data as json to the endpoint
    /// </summary>
    Task<TResult?> GetDataAsJsonAsync<TResult>(string requestUri);
}