namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;

public interface IClientRefreshTokenTimer : IDisposable
{
    Task StartRefreshTimerAsync();
    Task StopRefreshTimerAsync();
}