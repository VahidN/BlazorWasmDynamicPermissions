using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public static class NavigationManagerExtensions
{
    public static string GetCurrentRelativePath(this NavigationManager navigationManager)
    {
        if (navigationManager == null)
        {
            throw new ArgumentNullException(nameof(navigationManager));
        }

        return navigationManager.ToBaseRelativePath(navigationManager.Uri).NormalizeRelativePath();
    }

    public static string NormalizeRelativePath(this string? path)
    {
        path = path?.TrimStart('/', '#');
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "/";
        }

        return path.Trim();
    }
}