using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;

// NOTE: It needs `dotnet add package Microsoft.AspNetCore.Authorization`

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public static class DynamicClientPermissionsPolicyExtensions
{
    public static AuthorizationOptions AddClientPolicies(this AuthorizationOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.AddPolicy(CustomPolicies.DynamicClientPermission,
            policy => policy.Requirements.Add(new DynamicClientPermissionRequirement()));
        return options;
    }
}