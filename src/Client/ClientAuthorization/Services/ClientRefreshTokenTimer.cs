using System.Timers;
using Blazored.LocalStorage;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Timer = System.Timers.Timer;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

/// <summary>
///     We need to use the refresh-token before its being expired.
/// </summary>
public class ClientRefreshTokenTimer : IClientRefreshTokenTimer
{
    private const string RefreshTokenTimerId = nameof(RefreshTokenTimerId);
    private static readonly string TimerId = Guid.NewGuid().ToString("N");

    private readonly IBearerTokensStore _bearerTokensStore;
    private readonly ILocalStorageService _localStorageService;
    private readonly ILogger<ClientRefreshTokenTimer> _logger;
    private readonly IClientRefreshTokenService _refreshTokenService;
    private bool _isDisposed;
    private Timer? _timer;

    public ClientRefreshTokenTimer(
        ILocalStorageService localStorageService,
        IBearerTokensStore bearerTokensStore,
        IClientRefreshTokenService refreshTokenService,
        ILogger<ClientRefreshTokenTimer> logger)
    {
        _localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
        _bearerTokensStore = bearerTokensStore ?? throw new ArgumentNullException(nameof(bearerTokensStore));
        _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
        _logger = logger;
        _logger.LogInformation("Instantiating refresh timer `{TimerId}`.", TimerId);
    }

    public async Task StartRefreshTimerAsync()
    {
        CleanupCurrentTimer();
        var interval = await GetTimerIntervalAsync();
        CreateOneTimeTimer(interval);
        await SetTimerIsStartedAsync();
    }

    public async Task StopRefreshTimerAsync()
    {
        CleanupCurrentTimer();
        await RemoveTokensAsync();
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private ValueTask RemoveTokensAsync()
    {
        return _localStorageService.RemoveItemAsync(RefreshTokenTimerId);
    }

    private void CreateOneTimeTimer(TimeSpan? interval)
    {
        if (interval is null)
        {
            return;
        }

        _timer = new Timer(interval.Value.TotalMilliseconds);
        _timer.Elapsed += NotifyTimerElapsed;
        _timer.AutoReset = false;
        _timer.Start();
    }

    private async void NotifyTimerElapsed(object? source, ElapsedEventArgs e)
    {
        try
        {
            if (!await IsCurrentTimerActiveAsync())
            {
                CleanupCurrentTimer();
                _logger.LogInformation("Canceled refresh timer `{TimerId}`.", TimerId);
                return;
            }

            _logger.LogInformation("Running refresh timer `{TimerId}`.", TimerId);

            var tokens = await _refreshTokenService.TryRefreshTokenAsync();
            if (tokens is not null)
            {
                await StartRefreshTimerAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Exception: {Ex}", ex);
        }
    }

    private async Task<TimeSpan?> GetTimerIntervalAsync()
    {
        var jwtInfo = await _bearerTokensStore.GetBearerTokenAsync(BearerTokenType.AccessToken);
        if (jwtInfo?.ExpirationDateUtc is null)
        {
            return null;
        }

        var interval = jwtInfo.ExpirationDateUtc.Value - DateTime.UtcNow;
        return interval.Subtract(TimeSpan.FromSeconds(7));
    }

    private async Task<bool> IsCurrentTimerActiveAsync()
    {
        // The last timer wins
        var lastActiveTimer = await GetLastActiveTimerIdAsync();
        var isActive = string.Equals(lastActiveTimer, TimerId, StringComparison.Ordinal);


        if (!isActive)
        {
            _logger.LogInformation("Current timer `{TimerId}` is not active and will be cancelled.", TimerId);
        }

        return isActive;
    }

    private async Task SetTimerIsStartedAsync()
    {
        await _localStorageService.SetItemAsync(RefreshTokenTimerId, TimerId);
        _logger.LogInformation("Refresh timer `{TimerId}` has been started.", TimerId);
    }

    private async Task<string> GetLastActiveTimerIdAsync()
    {
        return await _localStorageService.GetItemAsync<string>(RefreshTokenTimerId) ?? string.Empty;
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            try
            {
                CleanupCurrentTimer();
            }
            finally
            {
                _isDisposed = true;
            }
        }
    }

    private void CleanupCurrentTimer()
    {
        if (_timer is null)
        {
            return;
        }

        _timer.Stop();
        _timer.Elapsed -= NotifyTimerElapsed;
        _timer.Dispose();
        _timer = null;
    }
}