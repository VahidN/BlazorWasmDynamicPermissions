using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

public class
    DynamicClientPermissionsAuthorizationHandler : AuthorizationHandler<DynamicClientPermissionRequirement>
{
    private readonly IClientSecurityTrimmingService _securityTrimmingService;

    public DynamicClientPermissionsAuthorizationHandler(IClientSecurityTrimmingService securityTrimmingService)
    {
        _securityTrimmingService =
            securityTrimmingService ?? throw new ArgumentNullException(nameof(securityTrimmingService));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, DynamicClientPermissionRequirement requirement)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (await _securityTrimmingService.HasUserAccessToProtectedPageAsync(context.User))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}