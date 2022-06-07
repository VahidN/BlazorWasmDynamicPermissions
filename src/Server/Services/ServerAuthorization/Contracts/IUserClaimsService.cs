using BlazorWasmDynamicPermissions.Server.Entities;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface IUserClaimsService
{
    Task<List<UserClaim>> GetUserClaimsAsync(int userId);
    Task<List<UserClaim>> GetUserClaimsAsync(int userId, string claimType, string claimValue);
    Task<ClaimsResponseDto> GetUserClaimsAsync(int userId, string claimType);
    Task<List<UserClaim>> GetUserClaimTypesAsync(int userId, string claimType);
    Task<bool> HasUserClaimTypeAsync(int userId, string claimType);
    Task<bool> HasUserClaimAsync(int userId, string claimType, string claimValue);
    Task<bool> HasCurrentUserClaimAsync(string claimType, string claimValue);
    Task<List<User>> GetUsersHaveUserClaimTypeAsync(string claimType);
    Task<List<User>> GetUsersHaveUserClaimAsync(string claimType, string claimValue);
    Task AddOrUpdateUserClaimsAsync(int userId, string claimType, IReadOnlyList<string>? inputClaimValues);
    Task<SecuredActionsDto> GetActionsWithDynamicServerPermission(int userId);
}