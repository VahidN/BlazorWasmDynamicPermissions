using BlazorWasmDynamicPermissions.Server.Entities;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface IUsersService
{
    Task<User?> FindUserAsync(string username, string password);
    ValueTask<User?> FindUserAsync(int userId);
    Task<UserDto?> GetUserDtoAsync(int userId);
    ValueTask<User?> GetCurrentUserAsync();
    int GetCurrentUserId();
    Task<(bool Succeeded, string Error)> ChangePasswordAsync(ChangePasswordDto? model);
    Task<List<UserDto>> GetAllUsersListAsync();
}