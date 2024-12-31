using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWasmDynamicPermissions.Server.Controllers.Identity;

[ApiController]
[Route(template: "api/[controller]")]
[EnableCors(policyName: "CorsPolicy")]
public class UsersAccountManagerController : ControllerBase
{
    private readonly ITokenFactoryService _tokenFactoryService;
    private readonly ITokenValidatorService _tokenValidatorService;
    private readonly IUsersService _usersService;
    private readonly IUserTokensService _userTokensService;

    public UsersAccountManagerController(IUsersService usersService,
        ITokenFactoryService tokenFactoryService,
        ITokenValidatorService tokenValidatorService,
        IUserTokensService userTokensService)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        _tokenFactoryService = tokenFactoryService ?? throw new ArgumentNullException(nameof(tokenFactoryService));

        _tokenValidatorService =
            tokenValidatorService ?? throw new ArgumentNullException(nameof(tokenValidatorService));

        _userTokensService = userTokensService ?? throw new ArgumentNullException(nameof(userTokensService));
    }

    [AllowAnonymous]
    [HttpPost(template: "[action]")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginUser)
    {
        if (loginUser is null)
        {
            return BadRequest(error: "User is not set.");
        }

        var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password);

        if (user?.IsActive != true)
        {
            return BadRequest(error: "This user is not active.");
        }

        var tokensResponseDto = await _tokenFactoryService.CreateTokensResponseDtoAsync(user);

        return Ok(tokensResponseDto);
    }

    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost(template: "[action]")]
    public async Task<IActionResult> RefreshToken([FromBody] UserTokensDto model)
    {
        var (tokensResponseDto, errorMessage) = await _tokenFactoryService.CreateRefreshTokenAsync(model);

        if (tokensResponseDto is null)
        {
            return BadRequest(errorMessage);
        }

        return Ok(tokensResponseDto);
    }

    [AllowAnonymous]
    [HttpGet(template: "[action]")]
    [HttpPost(template: "[action]")]
    public async Task<IActionResult> Logout()
    {
        // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
        // Delete the user's tokens from the database (revoke its bearer token)
        await _userTokensService.RevokeCurrentUserBearerTokensAsync();

        return Ok(new ApiResponseDto
        {
            Success = true
        });
    }

    [Authorize]
    [HttpPost(template: "[action]")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var (succeeded, error) = await _usersService.ChangePasswordAsync(model);

        return succeeded
            ? Ok(new ApiResponseDto
            {
                Success = true
            })
            : BadRequest(error);
    }

    [Authorize(Roles = CustomRoles.Admin)]
    [HttpGet(template: "[action]")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Users()
    {
        var users = await _usersService.GetAllUsersListAsync();

        return Ok(users);
    }

    [AllowAnonymous]
    [HttpPost(template: "[action]")]
    public async Task<IActionResult> ValidateAccessToken([FromBody] UserTokensDto model)
    {
        if (model is null)
        {
            return BadRequest(error: "Model is null.");
        }

        var (result, _, _) =
            await _tokenValidatorService.IsValidTokenAsync(model.AccessToken, BearerTokenType.AccessToken);

        return Ok(new ApiResponseDto
        {
            Success = result
        });
    }
}