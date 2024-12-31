using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorWasmDynamicPermissions.Client.Pages.Shared;

public partial class MainLayout
{
    private IReadOnlyList<DropdownMenuItem> UserMenuItems { get; } = new List<DropdownMenuItem>
    {
        new(title: "Change Password", url: "change-password", glyphIcon: "bi bi-lock"),
        new(title: "Logout", url: "logout", glyphIcon: "bi bi-door-open")
    };

    private ErrorBoundary? OurErrorBoundary { set; get; }

    protected override void OnParametersSet() => ResetBoundary();

    private void ResetBoundary()
        =>

            // On each page navigation, reset any error state
            OurErrorBoundary?.Recover();
}