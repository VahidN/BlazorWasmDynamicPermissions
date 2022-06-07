using BlazorWasmDynamicPermissions.Server.DataLayer.Context;
using BlazorWasmDynamicPermissions.Server.Entities;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class UserTokensService : IUserTokensService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsersService _usersService;

    public UserTokensService(ApplicationDbContext context, IUsersService usersService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
    }

    public async Task AddOrUpdateUserTokenAsync(int userId, string accessTokenSerial, string refreshTokenSerial)
    {
        var token = await GetUserTokenAsync(userId);
        if (token is null)
        {
            _context.UserTokens.Add(new UserToken
            {
                UserId = userId,
                AccessTokenSerialNumber = accessTokenSerial,
                RefreshTokenSerialNumber = refreshTokenSerial
            });
        }
        else
        {
            token.AccessTokenSerialNumber = accessTokenSerial;
            token.RefreshTokenSerialNumber = refreshTokenSerial;
        }

        await _context.SaveChangesAsync();
    }

    public Task<UserToken?> GetUserTokenAsync(int userId)
    {
        return _context.UserTokens.SingleOrDefaultAsync(userToken => userToken.UserId == userId);
    }

    public Task<UserToken?> GetUserTokenIncludeUserAsync(int userId)
    {
        return _context.UserTokens.Include(userToken => userToken.User)
            .SingleOrDefaultAsync(userToken => userToken.UserId == userId);
    }

    public Task<UserToken?> GetUserTokenIncludeUserAsync(string refreshTokenSerialNumber)
    {
        return _context.UserTokens.Include(userToken => userToken.User)
            .SingleOrDefaultAsync(userToken => userToken.RefreshTokenSerialNumber == refreshTokenSerialNumber);
    }

    public async Task RevokeCurrentUserBearerTokensAsync()
    {
        var userId = _usersService.GetCurrentUserId();
        var userToken = await GetUserTokenAsync(userId);
        if (userToken is null)
        {
            return;
        }

        _context.UserTokens.Remove(userToken);
        await _context.SaveChangesAsync();
    }
}