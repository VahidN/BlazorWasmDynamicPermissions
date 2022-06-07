using System.ComponentModel;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWasmDynamicPermissions.Server.Controllers.Tests;

[Authorize(Policy = CustomPolicies.DynamicServerPermission),
 Route("api/[controller]"),
 EnableCors("CorsPolicy"),
 DisplayName("A test controller with dynamic server-side permissions")]
public class DynamicServerPermissionsSampleController : Controller
{
    private readonly IUserClaimsService _userClaimsService;
    private readonly IUsersService _usersService;

    public DynamicServerPermissionsSampleController(
        IUsersService usersService,
        IUserClaimsService userClaimsService)
    {
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
        _userClaimsService = userClaimsService ?? throw new ArgumentNullException(nameof(userClaimsService));
    }

    [HttpGet("[action]"), HttpPost("[action]"),
     ResponseCache(Location = ResponseCacheLocation.None, NoStore = true),
     DisplayName("Get user's information")]
    public async Task<IActionResult> MyUserInfo()
    {
        var user = await _usersService.GetCurrentUserAsync();
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        return Ok(new UserDto
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            Id = user.Id
        });
    }

    [HttpGet("[action]"),
     ResponseCache(Location = ResponseCacheLocation.None, NoStore = true),
     DisplayName("Get user's dynamic claims")]
    public async Task<IActionResult> MyDynamicClientClaims()
    {
        var user = await _usersService.GetCurrentUserAsync();
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var permissions =
            await _userClaimsService.GetUserClaimsAsync(user.Id, CustomPolicies.DynamicClientPermission);
        return Ok(permissions);
    }
}