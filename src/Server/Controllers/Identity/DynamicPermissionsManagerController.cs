using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWasmDynamicPermissions.Server.Controllers.Identity;

[Authorize(Roles = CustomRoles.Admin),
 Route("api/[controller]"),
 EnableCors("CorsPolicy")]
public class DynamicPermissionsManagerController : Controller
{
    private readonly IUserClaimsService _userClaimsService;

    public DynamicPermissionsManagerController(IUserClaimsService userClaimsService)
    {
        _userClaimsService = userClaimsService ?? throw new ArgumentNullException(nameof(userClaimsService));
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> AddOrUpdateClaims([FromBody] DynamicClaimsDto model)
    {
        if (model is null)
        {
            return BadRequest("Model is null.");
        }

        await _userClaimsService.AddOrUpdateUserClaimsAsync(
            model.UserId, model.ClaimType, model.InputClaimValues);
        return Ok(new ApiResponseDto { Success = true });
    }

    [HttpGet("[action]/{userId:int}"),
     ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> UserDynamicClientPermissions(int userId)
    {
        var permissions =
            await _userClaimsService.GetUserClaimsAsync(userId, CustomPolicies.DynamicClientPermission);
        return Ok(permissions);
    }

    [HttpGet("[action]/{userId:int}"),
     ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> DynamicallySecuredServerActions(int userId)
    {
        var securedActions = await _userClaimsService.GetActionsWithDynamicServerPermission(userId);
        return Ok(securedActions);
    }
}