using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmDynamicPermissions.Client.Pages.Identity;

public partial class Logout
{
    [Inject] private NavigationManager NavigationManager { set; get; } = default!;

    [Inject] private IBearerTokensStore BearerTokensStore { set; get; } = default!;

    [Inject] private AuthenticationStateProvider AuthStateProvider { set; get; } = default!;

    [Inject] private IHttpClientService HttpClientService { set; get; } = default!;

    [Inject] private IClientRefreshTokenTimer RefreshTokenTimer { set; get; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var response =
            await HttpClientService.GetDataAsJsonAsync<ApiResponseDto>(requestUri: "api/UsersAccountManager/Logout");

        if (response?.Success == true)
        {
            await BearerTokensStore.RemoveAllBearerTokensAsync();
            await RefreshTokenTimer.StopRefreshTimerAsync();
            ((ClientAuthenticationStateProvider)AuthStateProvider).NotifyUserLogout();
            NavigationManager.NavigateTo(uri: "");
        }
    }
}