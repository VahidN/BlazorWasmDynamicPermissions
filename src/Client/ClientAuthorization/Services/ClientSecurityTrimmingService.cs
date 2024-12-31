using System.Security.Claims;
using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class ClientSecurityTrimmingService(
    IDynamicClientPermissionsProvider dynamicClientPermissionsProvider,
    NavigationManager navigationManager) : IClientSecurityTrimmingService
{
    public async Task<bool> HasUserAccessToProtectedPageAsync(ClaimsPrincipal? user)
    {
        var dynamicPermissions = await dynamicClientPermissionsProvider.GetDynamicPermissionClaimsAsync();
        var path = navigationManager.GetCurrentRelativePath();

        return HasUserAccessToProtectedPage(user, dynamicPermissions, path);
    }

    public bool HasUserAccessToProtectedPage(ClaimsPrincipal? user, ClaimsResponseDto? dynamicPermissions, string? path)
    {
        if (!user.IsAuthenticated())
        {
            return false;
        }

        if (user.HasUserClaims(new Claim(ClaimTypes.Role, CustomRoles.Admin)))
        {
            // Admin users have access to all the pages.
            return true;
        }

        if (dynamicPermissions?.ClaimValues is null || dynamicPermissions.ClaimValues.Count == 0)
        {
            return false;
        }

        return dynamicPermissions.ClaimValues.Contains(path, StringComparer.OrdinalIgnoreCase);
    }
}