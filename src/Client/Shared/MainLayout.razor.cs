using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorWasmDynamicPermissions.Client.Shared;

public partial class MainLayout
{
    private IReadOnlyList<DropdownMenuItem> UserMenuItems { get; } =
        new List<DropdownMenuItem>
        {
            new("Change Password", "change-password", "bi bi-lock"),
            new("Logout", "logout", "bi bi-door-open")
        };

    private ErrorBoundary? OurErrorBoundary { set; get; }

    protected override void OnParametersSet()
    {
        ResetBoundary();
    }

    private void ResetBoundary()
    {
        // On each page navigation, reset any error state
        OurErrorBoundary?.Recover();
    }
}