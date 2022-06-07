using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlazorWasmDynamicPermissions.Server.Entities;
using BlazorWasmDynamicPermissions.Server.Models.SiteOptions;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class TokenFactoryService : ITokenFactoryService
{
    private readonly BearerTokenOptionDto _bearerTokenOption;
    private readonly ISecurityService _securityService;
    private readonly ITokenValidatorService _tokenValidatorService;
    private readonly IUserClaimsService _userClaimsService;
    private readonly IUserTokensService _userTokensService;

    public TokenFactoryService(
        ISecurityService securityService,
        IUserClaimsService userClaimsService,
        IOptionsSnapshot<SiteSettingsDto> configuration,
        IUserTokensService userTokensService,
        ITokenValidatorService tokenValidatorService)
    {
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _userClaimsService = userClaimsService ?? throw new ArgumentNullException(nameof(userClaimsService));
        _userTokensService = userTokensService ?? throw new ArgumentNullException(nameof(userTokensService));
        _tokenValidatorService =
            tokenValidatorService ?? throw new ArgumentNullException(nameof(tokenValidatorService));

        if (configuration is null || configuration.Value is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        _bearerTokenOption = configuration.Value.BearerToken ??
                             throw new InvalidOperationException("bearerTokenOption is null");
    }

    public async Task<(string Token, string TokenSerial)> CreateIdentityAccessTokenAsync(User user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var (claims, tokenSerial) = await GetUserClaimsAsync(user);
        return (CreateJwtSecurityToken(claims, _bearerTokenOption.AccessTokenExpirationMinutes),
            tokenSerial);
    }

    public string CreateJwtSecurityToken(IList<Claim> claims, int expirationMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearerTokenOption.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            _bearerTokenOption.Issuer,
            _bearerTokenOption.Audience,
            claims,
            now,
            now.AddMinutes(expirationMinutes),
            credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> CreateDynamicClientPermissionsTokenAsync(User user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var permissions =
            await _userClaimsService.GetUserClaimsAsync(user.Id, CustomPolicies.DynamicClientPermission);
        var claims = new List<Claim>
        {
            // Issuer
            new(JwtRegisteredClaimNames.Iss, _bearerTokenOption.Issuer, ClaimValueTypes.String,
                _bearerTokenOption.Issuer),
            // Issued at
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64, _bearerTokenOption.Issuer),
            new(CustomPolicies.DynamicClientPermission, JsonSerializer.Serialize(permissions),
                ClaimValueTypes.String,
                _bearerTokenOption.Issuer),
            // custom data
            new(ClaimTypes.UserData, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String,
                _bearerTokenOption.Issuer)
        };
        return CreateJwtSecurityToken(claims, _bearerTokenOption.AccessTokenExpirationMinutes);
    }

    public (string Token, string TokenSerial) CreateRefreshToken(int userId)
    {
        var refreshTokenSerial = _securityService.CreateCryptographicallySecureGuid().ToString("N");
        var claims = new List<Claim>
        {
            // Unique Id for all Jwt tokes
            new(JwtRegisteredClaimNames.Jti, refreshTokenSerial, ClaimValueTypes.String, _bearerTokenOption.Issuer),
            // Issuer
            new(JwtRegisteredClaimNames.Iss, _bearerTokenOption.Issuer, ClaimValueTypes.String,
                _bearerTokenOption.Issuer),
            // Issued at
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64, _bearerTokenOption.Issuer),
            // custom data
            new(ClaimTypes.UserData, userId.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String,
                _bearerTokenOption.Issuer)
        };
        var refreshTokenValue = CreateJwtSecurityToken(claims, _bearerTokenOption.RefreshTokenExpirationMinutes);
        return (refreshTokenValue, refreshTokenSerial);
    }

    public async Task<(UserTokensDto? Tokens, string ErrorMessage)> CreateRefreshTokenAsync(UserTokensDto? model)
    {
        if (model is null)
        {
            return (null, "The provided data is null.");
        }

        var refreshTokenValue = model.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            return (null, "The provided token is null.");
        }

        var (refreshTokenSerialNumber, message) =
            await _tokenValidatorService.GetTokenSerialNumberAsync(refreshTokenValue, BearerTokenType.RefreshToken);
        if (string.IsNullOrWhiteSpace(refreshTokenSerialNumber))
        {
            return (null, $"Couldn't retrieve the token's serial number. {message}");
        }

        var token = await _userTokensService.GetUserTokenIncludeUserAsync(refreshTokenSerialNumber);
        if (token is null)
        {
            return (null, "This token doesn't belong to any user.");
        }

        var tokensResponseDto = await CreateTokensResponseDtoAsync(token.User);
        return (tokensResponseDto, "");
    }

    public async Task<UserTokensDto> CreateTokensResponseDtoAsync(User user)
    {
        var (accessToken, accessTokenSerial) = await CreateIdentityAccessTokenAsync(user);
        var permissionsToken = await CreateDynamicClientPermissionsTokenAsync(user);
        var (refreshToken, refreshTokenSerial) = CreateRefreshToken(user.Id);

        await _userTokensService.AddOrUpdateUserTokenAsync(user.Id, accessTokenSerial, refreshTokenSerial);

        var tokensResponseDto = new UserTokensDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            DynamicPermissionsToken = permissionsToken // Making it harder to be modified on the client-side!
        };
        return tokensResponseDto;
    }

    private async Task<(List<Claim>Claims, string TokenSerial)> GetUserClaimsAsync(User user)
    {
        var tokenSerial = _securityService.CreateCryptographicallySecureGuid().ToString("N");
        var claims = new List<Claim>
        {
            // Unique Id for all Jwt tokes
            new(JwtRegisteredClaimNames.Jti, tokenSerial, ClaimValueTypes.String, _bearerTokenOption.Issuer),
            // Issuer
            new(JwtRegisteredClaimNames.Iss, _bearerTokenOption.Issuer, ClaimValueTypes.String,
                _bearerTokenOption.Issuer),
            // Issued at
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64, _bearerTokenOption.Issuer),
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String,
                _bearerTokenOption.Issuer),
            new(ClaimTypes.Name, user.Username, ClaimValueTypes.String, _bearerTokenOption.Issuer),
            new("DisplayName", user.DisplayName ?? "", ClaimValueTypes.String, _bearerTokenOption.Issuer),
            // custom data
            new(ClaimTypes.UserData, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.String,
                _bearerTokenOption.Issuer)
        };

        // add roles
        var roles = await _userClaimsService.GetUserClaimTypesAsync(user.Id, ClaimTypes.Role);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ClaimValue, ClaimValueTypes.String,
                _bearerTokenOption.Issuer));
        }

        return (claims, tokenSerial);
    }
}