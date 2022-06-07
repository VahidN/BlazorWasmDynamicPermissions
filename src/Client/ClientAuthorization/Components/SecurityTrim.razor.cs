using System.Security.Claims;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Components;

/// <summary>
///     A custom security trimming component
/// </summary>
public partial class SecurityTrim
{
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationState { set; get; }
    private AuthenticationState? AuthState { set; get; }

    private bool IsVisible => AuthState.HasUserAccess(AllowAnonymous, AllowedClaims, AllowedRoles);

    /// <summary>
    ///     The descendant components (defined in ChildContent) to receive the specified CascadingValue.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    ///     Is this item visible to anonymous/unauthenticated users?
    ///     Its default value is `true`.
    /// </summary>
    [Parameter]
    public bool AllowAnonymous { set; get; } = true;

    /// <summary>
    ///     Which comma separated roles are allowed to see this menu item?
    /// </summary>
    [Parameter]
    public string? AllowedRoles { set; get; }

    /// <summary>
    ///     Which user-claims are allowed to see this menu item?
    /// </summary>
    [Parameter]
    public IReadOnlyList<Claim>? AllowedClaims { set; get; }

    /// <summary>
    ///     Method invoked when the component is ready to start
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is null)
        {
            return;
        }

        AuthState = await AuthenticationState;
    }
}