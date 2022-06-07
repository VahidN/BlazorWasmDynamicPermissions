using System.Security.Claims;
using BlazorWasmDynamicPermissions.Server.Entities;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface ITokenFactoryService
{
    Task<(string Token, string TokenSerial)> CreateIdentityAccessTokenAsync(User user);
    (string Token, string TokenSerial) CreateRefreshToken(int userId);
    Task<(UserTokensDto? Tokens, string ErrorMessage)> CreateRefreshTokenAsync(UserTokensDto? model);
    string CreateJwtSecurityToken(IList<Claim> claims, int expirationMinutes);
    Task<string> CreateDynamicClientPermissionsTokenAsync(User user);
    Task<UserTokensDto> CreateTokensResponseDtoAsync(User user);
}