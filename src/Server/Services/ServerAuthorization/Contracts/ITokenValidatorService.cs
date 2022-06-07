using System.Security.Claims;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface ITokenValidatorService
{
    Task<(string? SerialNumber, string Message)> GetTokenSerialNumberAsync(string token, BearerTokenType tokenType);
    Task ValidateAccessTokenAsync(TokenValidatedContext context);

    Task<(bool IsValid, ClaimsPrincipal? ClaimsPrincipal, string Message)> IsValidTokenAsync(string? token,
        BearerTokenType tokenType);
}