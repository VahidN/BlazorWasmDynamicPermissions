using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlazorWasmDynamicPermissions.Server.Models.SiteOptions;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class TokenValidatorService : ITokenValidatorService
{
    private readonly BearerTokenOptionDto _configuration;
    private readonly IUserTokensService _userTokensService;

    public TokenValidatorService(
        IUserTokensService userTokensService,
        IOptionsSnapshot<SiteSettingsDto> configuration)
    {
        _userTokensService = userTokensService ?? throw new ArgumentNullException(nameof(userTokensService));

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        _configuration = configuration.Value?.BearerToken ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task ValidateAccessTokenAsync(TokenValidatedContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var (success, message) =
            await IsValidClaimsPrincipalAsync(context.Principal, context.SecurityToken, BearerTokenType.AccessToken);
        if (!success)
        {
            context.Fail(message);
        }
    }

    public async Task<(bool IsValid, ClaimsPrincipal? ClaimsPrincipal, string Message)> IsValidTokenAsync(
        string? token, BearerTokenType tokenType)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidIssuer = _configuration.Issuer, // site that makes the token
                ValidateIssuer = true,
                ValidAudience = _configuration.Audience, // site that consumes the token
                ValidateAudience = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Key)),
                ValidateIssuerSigningKey = true, // verify signature to avoid tampering
                ValidateLifetime = true, // validate the expiration
                ClockSkew = TimeSpan.Zero // tolerance for the expiration date
            }, out var securityToken);

            var (success, message) =
                await IsValidClaimsPrincipalAsync(claimsPrincipal, securityToken, tokenType);
            return (success, claimsPrincipal, message);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(string? SerialNumber, string Message)> GetTokenSerialNumberAsync(string token,
        BearerTokenType tokenType)
    {
        var (isValid, claimsPrincipal, message) = await IsValidTokenAsync(token, tokenType);
        if (!isValid)
        {
            return (null, message);
        }

        var serialNumber = claimsPrincipal?.Claims?.FirstOrDefault(
            c => string.Equals(c.Type, JwtRegisteredClaimNames.Jti, StringComparison.Ordinal))?.Value;
        return (serialNumber, "");
    }

    private async Task<(bool Success, string Message)> IsValidClaimsPrincipalAsync(
        ClaimsPrincipal? claimsPrincipal, SecurityToken securityToken, BearerTokenType tokenType)
    {
        var claimsIdentity = claimsPrincipal?.Identity as ClaimsIdentity;
        if (claimsIdentity?.Claims is null || !claimsIdentity.Claims.Any())
        {
            return (false, "This is not our issued token. It has no claims.");
        }

        var userIdString = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
        if (!int.TryParse(userIdString, NumberStyles.Number, CultureInfo.InvariantCulture, out var userId))
        {
            return (false, "This is not our issued token. It has no user-id.");
        }

        var userToken = await _userTokensService.GetUserTokenIncludeUserAsync(userId);
        if (userToken?.User is not { IsActive: true })
        {
            // user has changed his/her password/roles/stat/IsActive
            return (false, "This user is not active anymore.");
        }

        var serialNumberClaim = claimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (serialNumberClaim is null)
        {
            return (false, "This is not our issued token. It has no serial.");
        }

        var dbSerialNUmber = tokenType == BearerTokenType.AccessToken
            ? userToken.AccessTokenSerialNumber
            : userToken.RefreshTokenSerialNumber;
        if (!string.Equals(dbSerialNUmber, serialNumberClaim, StringComparison.Ordinal))
        {
            return (false, "This token is expired. Please login again.");
        }

        if (securityToken is not JwtSecurityToken accessToken ||
            string.IsNullOrWhiteSpace(accessToken.RawData))
        {
            return (false, "This token is not in our database.");
        }

        return (true, "");
    }
}