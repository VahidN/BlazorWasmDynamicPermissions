using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class DynamicClientPermissionsAuthorizationHandler(IClientSecurityTrimmingService securityTrimmingService)
    : AuthorizationHandler<DynamicClientPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        DynamicClientPermissionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (await securityTrimmingService.HasUserAccessToProtectedPageAsync(context.User))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}