using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Components;

/// <summary>
///     Renders all of the available RoutablePages
/// </summary>
public partial class NavBarDynamicMenus
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { set; get; }
    private AuthenticationState? AuthState { set; get; }

    [Inject] private IDynamicClientPermissionsProvider DynamicClientPermissionsProvider { set; get; } = default!;

    [Inject] private IClientSecurityTrimmingService SecurityTrimmingService { set; get; } = default!;

    [Inject] private IProtectedPagesProvider ProtectedPagesProvider { set; get; } = default!;

    private ClaimsResponseDto? DynamicPermissions { set; get; }

    private IReadOnlyList<ProtectedPageAttribute> GetAllowedMenuItems(GroupedProtectedPage mainMenuItem)
    {
        return mainMenuItem.GroupItems.Where(
            pageAttribute => SecurityTrimmingService.HasUserAccessToProtectedPage(
                AuthState?.User,
                DynamicPermissions,
                pageAttribute.Url)).ToList();
    }

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is null)
        {
            return;
        }

        AuthState = await AuthenticationState;
        DynamicPermissions = await DynamicClientPermissionsProvider.GetDynamicPermissionClaimsAsync();
    }
}