using BlazorWasmDynamicPermissions.Server.Entities;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface IUserTokensService
{
    Task AddOrUpdateUserTokenAsync(int userId, string accessTokenSerial, string refreshTokenSerial);
    Task<UserToken?> GetUserTokenAsync(int userId);
    Task<UserToken?> GetUserTokenIncludeUserAsync(int userId);
    Task<UserToken?> GetUserTokenIncludeUserAsync(string refreshTokenSerialNumber);
    Task RevokeCurrentUserBearerTokensAsync();
}