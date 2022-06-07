using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class DynamicClientPermissionsProvider : IDynamicClientPermissionsProvider
{
    private readonly IBearerTokensStore _bearerTokensStore;

    public DynamicClientPermissionsProvider(IBearerTokensStore bearerTokensStore)
    {
        _bearerTokensStore = bearerTokensStore ?? throw new ArgumentNullException(nameof(bearerTokensStore));
    }

    /// <summary>
    ///     Returns the list of the Dynamic Client Permissions.
    /// </summary>
    public async Task<ClaimsResponseDto?> GetDynamicPermissionClaimsAsync()
    {
        var jwtInfo = await _bearerTokensStore.GetBearerTokenAsync(BearerTokenType.DynamicPermissions);
        var dynamicPermissionsClaimValue = jwtInfo?.Claims.FirstOrDefault(claim =>
            string.Equals(claim.Type, CustomPolicies.DynamicClientPermission, StringComparison.Ordinal))?.Value;
        return string.IsNullOrWhiteSpace(dynamicPermissionsClaimValue)
            ? null
            : JsonSerializer.Deserialize<ClaimsResponseDto?>(dynamicPermissionsClaimValue,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
    }
}