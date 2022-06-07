using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmDynamicPermissions.Client.Pages.Identity;

public partial class Login
{
    private UserLoginDto Model { get; } = new();

    [Inject] private IHttpClientService HttpClientService { set; get; } = default!;

    [Inject] private IBearerTokensStore BearerTokensStore { set; get; } = default!;

    [Inject] private NavigationManager NavigationManager { set; get; } = default!;

    [Inject] private AuthenticationStateProvider AuthStateProvider { set; get; } = default!;

    [Inject] private IClientRefreshTokenTimer RefreshTokenTimer { set; get; } = default!;

    [Parameter, SupplyParameterFromQuery] public string? ReturnUrl { set; get; }

    private async Task LoginUserAsync()
    {
        var response =
            await HttpClientService.PostDataAsJsonAsync<UserTokensDto>("api/UsersAccountManager/login", Model);
        await BearerTokensStore.StoreAllTokensAsync(response);
        await ((ClientAuthenticationStateProvider)AuthStateProvider).NotifyUserLoggedInAsync();
        await RefreshTokenTimer.StartRefreshTimerAsync();
        RedirectAfterLogin();
    }

    private void RedirectAfterLogin()
    {
        NavigationManager.NavigateTo(string.IsNullOrEmpty(ReturnUrl) ? "" : $"{ReturnUrl}");
    }
}