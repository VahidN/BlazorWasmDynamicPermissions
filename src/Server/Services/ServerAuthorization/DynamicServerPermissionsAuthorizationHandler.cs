using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class DynamicServerPermissionsAuthorizationHandler : AuthorizationHandler<DynamicServerPermissionRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServerSecurityTrimmingService _serverSecurityTrimmingService;

    public DynamicServerPermissionsAuthorizationHandler(
        IServerSecurityTrimmingService serverSecurityTrimmingService,
        IHttpContextAccessor httpContextAccessor)
    {
        _serverSecurityTrimmingService =
            serverSecurityTrimmingService ?? throw new ArgumentNullException(nameof(serverSecurityTrimmingService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, DynamicServerPermissionRequirement requirement)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var httpMethod = httpContext?.Request.Method ?? "any";
        var routeData = httpContext?.GetRouteData();
        var area = GetRouteDataValue(routeData, "area");
        var controller = GetRouteDataValue(routeData, "controller");
        var action = GetRouteDataValue(routeData, "action");

        if (await _serverSecurityTrimmingService.CanCurrentUserAccessToActionAsync(
                area, controller, action, httpMethod))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

    private static string GetRouteDataValue(RouteData? routeData, string key)
    {
        var value = routeData?.Values[key]?.ToString();
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }
}