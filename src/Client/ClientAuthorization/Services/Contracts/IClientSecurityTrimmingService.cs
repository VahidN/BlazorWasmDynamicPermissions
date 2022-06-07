using System.Security.Claims;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;

public interface IClientSecurityTrimmingService
{
    Task<bool> HasUserAccessToProtectedPageAsync(ClaimsPrincipal? user);
    bool HasUserAccessToProtectedPage(ClaimsPrincipal? user, ClaimsResponseDto? dynamicPermissions, string? path);
}