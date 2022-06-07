namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface IServerSecurityTrimmingService
{
    Task<bool> CanCurrentUserAccessToActionAsync(
        string area, string controller, string action, string httpMethod);
}