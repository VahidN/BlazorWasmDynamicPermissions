using System.Security.Claims;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class ClientSecurityTrimmingService : IClientSecurityTrimmingService
{
    private readonly IDynamicClientPermissionsProvider _dynamicClientPermissionsProvider;
    private readonly NavigationManager _navigationManager;

    public ClientSecurityTrimmingService(
        IDynamicClientPermissionsProvider dynamicClientPermissionsProvider,
        NavigationManager navigationManager)
    {
        _dynamicClientPermissionsProvider = dynamicClientPermissionsProvider ??
                                            throw new ArgumentNullException(nameof(dynamicClientPermissionsProvider));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    public async Task<bool> HasUserAccessToProtectedPageAsync(ClaimsPrincipal? user)
    {
        var dynamicPermissions = await _dynamicClientPermissionsProvider.GetDynamicPermissionClaimsAsync();
        var path = _navigationManager.GetCurrentRelativePath();
        return HasUserAccessToProtectedPage(user, dynamicPermissions, path);
    }

    public bool HasUserAccessToProtectedPage(
        ClaimsPrincipal? user, ClaimsResponseDto? dynamicPermissions, string? path)
    {
        if (!user.IsAuthenticated())
        {
            return false;
        }

        if (user.HasUserClaims(new Claim(ClaimTypes.Role, CustomRoles.Admin)))
        {
            // Admin users have access to all of the pages.
            return true;
        }

        if (dynamicPermissions?.ClaimValues is null || dynamicPermissions.ClaimValues.Count == 0)
        {
            return false;
        }

        return dynamicPermissions.ClaimValues.Contains(path, StringComparer.OrdinalIgnoreCase);
    }
}