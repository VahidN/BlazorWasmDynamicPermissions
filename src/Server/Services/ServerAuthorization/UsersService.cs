using System.Security.Claims;
using BlazorWasmDynamicPermissions.Server.DataLayer.Context;
using BlazorWasmDynamicPermissions.Server.Entities;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class UsersService : IUsersService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ISecurityService _securityService;

    public UsersService(
        ApplicationDbContext context,
        ISecurityService securityService,
        IHttpContextAccessor contextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }

    public ValueTask<User?> FindUserAsync(int userId)
    {
        return _context.Users.FindAsync(userId);
    }

    public Task<User?> FindUserAsync(string username, string password)
    {
        var passwordHash = _securityService.GetSha256Hash(password);
        return _context.Users.FirstOrDefaultAsync(x => x.Username == username && x.Password == passwordHash);
    }

    public int GetCurrentUserId()
    {
        var claimsIdentity = _contextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        var userDataClaim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
        var userId = userDataClaim?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? 0
            : int.Parse(userId, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    public ValueTask<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        return FindUserAsync(userId);
    }

    public async Task<(bool Succeeded, string Error)> ChangePasswordAsync(ChangePasswordDto? model)
    {
        if (model is null)
        {
            return (false, "Model is null.");
        }

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return (false, "User not found.");
        }

        var currentPasswordHash = _securityService.GetSha256Hash(model.OldPassword);
        if (!string.Equals(user.Password, currentPasswordHash, StringComparison.Ordinal))
        {
            return (false, "Current password is wrong.");
        }

        user.Password = _securityService.GetSha256Hash(model.NewPassword);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    public Task<List<UserDto>> GetAllUsersListAsync()
    {
        return _context.Users.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName
        }).ToListAsync();
    }

    public Task<UserDto?> GetUserDtoAsync(int userId)
    {
        return _context.Users.Where(user => user.Id == userId).Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName
        }).FirstOrDefaultAsync();
    }
}