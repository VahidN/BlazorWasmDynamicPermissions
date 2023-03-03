using System.ComponentModel;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class ApiActionsDiscoveryService : IApiActionsDiscoveryService
{
    private readonly IActionDescriptorCollectionProvider _actionsProvider;

    public ApiActionsDiscoveryService(IActionDescriptorCollectionProvider actionsProvider)
    {
        _actionsProvider = actionsProvider ?? throw new ArgumentNullException(nameof(actionsProvider));
        DynamicallySecuredActions = GetDynamicallySecuredActions().ToList();
    }

    public IReadOnlyList<ApiControllerDto> DynamicallySecuredActions { get; }

    private HashSet<ApiControllerDto> GetDynamicallySecuredActions()
    {
        var apiControllers = new HashSet<ApiControllerDto>(new ApiControllerEqualityComparer());

        foreach (var descriptor in _actionsProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>())
        {
            var controllerTypeInfo = descriptor.ControllerTypeInfo;
            var actionMethodInfo = descriptor.MethodInfo;

            if (!HasDynamicServerPermission(controllerTypeInfo, actionMethodInfo))
            {
                continue;
            }

            var controllerAttributes = controllerTypeInfo.GetCustomAttributes(true);
            var currentController = new ApiControllerDto
            {
                AreaName = controllerAttributes.OfType<AreaAttribute>().FirstOrDefault()?.RouteValue ?? string.Empty,
                ControllerDisplayName = GetDisplayName(controllerAttributes),
                ControllerName = descriptor.ControllerName
            };
            if (apiControllers.TryGetValue(currentController, out var actualController))
            {
                currentController = actualController;
            }
            else
            {
                apiControllers.Add(currentController);
            }

            var actionMethodAttributes = actionMethodInfo.GetCustomAttributes(true);
            var mvcActionItem = new ApiActionDto
            {
                ActionName = descriptor.ActionName,
                ActionDisplayName = GetDisplayName(actionMethodAttributes),
                HttpMethods =
                    descriptor.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods ??
                    new[] { "any" }
            };
            currentController.ApiActions.Add(mvcActionItem);
        }

        return apiControllers;
    }

    private static string GetDisplayName(object[] attributes)
    {
        return attributes.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName ??
               attributes.OfType<DisplayAttribute>().FirstOrDefault()?.Name ?? string.Empty;
    }

    private static bool HasDynamicServerPermission(MemberInfo controllerTypeInfo, MemberInfo actionMethodInfo)
    {
        var actionHasAllowAnonymousAttribute =
            actionMethodInfo.GetCustomAttribute<AllowAnonymousAttribute>(true) != null;
        if (actionHasAllowAnonymousAttribute)
        {
            return false;
        }

        var controllerAuthorizeAttribute = controllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>(true);
        if (IsDynamicallySecured(controllerAuthorizeAttribute))
        {
            return true;
        }

        var actionMethodAuthorizeAttribute = actionMethodInfo.GetCustomAttribute<AuthorizeAttribute>(true);
        if (IsDynamicallySecured(actionMethodAuthorizeAttribute))
        {
            return true;
        }

        return false;
    }

    private static bool IsDynamicallySecured(AuthorizeAttribute? controllerAuthorizeAttribute)
    {
        return controllerAuthorizeAttribute is not null && string.Equals(controllerAuthorizeAttribute.Policy,
            CustomPolicies.DynamicServerPermission, StringComparison.Ordinal);
    }
}