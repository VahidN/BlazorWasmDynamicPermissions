using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class ServerSecurityTrimmingService : IServerSecurityTrimmingService
{
    private readonly IApiActionsDiscoveryService _actionsDiscoveryService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserClaimsService _userClaimsService;

    public ServerSecurityTrimmingService(
        IHttpContextAccessor httpContextAccessor,
        IApiActionsDiscoveryService actionsDiscoveryService,
        IUserClaimsService userClaimsService)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _actionsDiscoveryService =
            actionsDiscoveryService ?? throw new ArgumentNullException(nameof(actionsDiscoveryService));
        _userClaimsService = userClaimsService ?? throw new ArgumentNullException(nameof(userClaimsService));
    }

    public Task<bool> CanCurrentUserAccessToActionAsync(
        string area, string controller, string action, string httpMethod)
    {
        var currentClaimValue = $"{area}:{controller}:{action}:{httpMethod}";
        CheckActionMethodExists(area, controller, action, currentClaimValue);

        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            return Task.FromResult(false);
        }

        if (user.IsInRole(CustomRoles.Admin))
        {
            // Admin users have access to all of the pages.
            return Task.FromResult(true);
        }

        return _userClaimsService.HasCurrentUserClaimAsync(CustomPolicies.DynamicServerPermission, currentClaimValue);
    }

    private void CheckActionMethodExists(string area, string controller, string action, string currentClaimValue)
    {
        if (!_actionsDiscoveryService.DynamicallySecuredActions
                .SelectMany(apiController => apiController.ApiActions,
                    (apiController, apiAction) => GetActionId(apiController, apiAction))
                .Any(actionId => string.Equals(actionId, currentClaimValue, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"The `secured` area={area}/controller={controller}/action={action} with `CustomPolicies.DynamicServerPermission` policy not found. Please check you have entered the area/controller/action names correctly and also it's decorated with the correct security policy.");
        }
    }

    private static string GetActionId(ApiControllerDto apiController, ApiActionDto apiAction)
    {
        return
            $"{apiController.AreaName}:{apiController.ControllerName}:{apiAction.ActionName}:{string.Join(",", apiAction.HttpMethods)}";
    }
}