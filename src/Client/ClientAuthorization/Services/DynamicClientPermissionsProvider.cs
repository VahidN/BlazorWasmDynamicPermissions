using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class DynamicClientPermissionsProvider(IBearerTokensStore bearerTokensStore) : IDynamicClientPermissionsProvider
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    ///     Returns the list of the Dynamic Client Permissions.
    /// </summary>
    public async Task<ClaimsResponseDto?> GetDynamicPermissionClaimsAsync()
    {
        var jwtInfo = await bearerTokensStore.GetBearerTokenAsync(BearerTokenType.DynamicPermissions);

        var dynamicPermissionsClaimValue = jwtInfo?.Claims.FirstOrDefault(claim
                => string.Equals(claim.Type, CustomPolicies.DynamicClientPermission, StringComparison.Ordinal))
            ?.Value;

        return string.IsNullOrWhiteSpace(dynamicPermissionsClaimValue)
            ? null
            : JsonSerializer.Deserialize<ClaimsResponseDto?>(dynamicPermissionsClaimValue, JsonSerializerOptions);
    }
}