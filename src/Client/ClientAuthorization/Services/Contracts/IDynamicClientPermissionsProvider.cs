using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;

public interface IDynamicClientPermissionsProvider
{
    /// <summary>
    ///     Returns the list of the Dynamic Client Permissions.
    /// </summary>
    Task<ClaimsResponseDto?> GetDynamicPermissionClaimsAsync();
}