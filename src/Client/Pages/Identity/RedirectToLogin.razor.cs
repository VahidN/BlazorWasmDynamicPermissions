using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmDynamicPermissions.Client.Pages.Identity;

public partial class RedirectToLogin
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { set; get; }

    [Inject] private NavigationManager NavigationManager { set; get; } = default!;

    [Inject] private IDynamicClientPermissionsProvider DynamicClientPermissionsProvider { set; get; } = default!;

    private AuthenticationState? AuthState { set; get; }

    private ClaimsResponseDto? DynamicPermissions { set; get; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is null)
        {
            return;
        }

        AuthState = await AuthenticationState;
        DynamicPermissions = await DynamicClientPermissionsProvider.GetDynamicPermissionClaimsAsync();

        if (AuthState.IsAuthenticated())
        {
            return;
        }

        var returnUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        NavigationManager.NavigateTo(string.IsNullOrEmpty(returnUrl)
            ? "login"
            : $"login?returnUrl={Uri.EscapeDataString(returnUrl)}");
    }
}