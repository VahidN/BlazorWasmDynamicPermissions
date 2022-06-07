using System.Security.Claims;
using BlazorWasmDynamicPermissions.Server.DataLayer.Context;
using BlazorWasmDynamicPermissions.Server.Entities;
using BlazorWasmDynamicPermissions.Server.Models.SiteOptions;
using BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization;

public class DbInitializerService : IDbInitializerService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISecurityService _securityService;
    private readonly IOptions<SiteSettingsDto> _siteSettingsOption;

    public DbInitializerService(
        IServiceScopeFactory scopeFactory,
        ISecurityService securityService,
        IOptions<SiteSettingsDto> siteSettingsOption)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _siteSettingsOption = siteSettingsOption ?? throw new ArgumentNullException(nameof(siteSettingsOption));
    }

    public void Initialize()
    {
        using var serviceScope = _scopeFactory.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    public void SeedData()
    {
        using var serviceScope = _scopeFactory.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Add default roles
        var adminRoleClaim = new UserClaim { ClaimType = ClaimTypes.Role, ClaimValue = CustomRoles.Admin };
        var userRoleClaim = new UserClaim { ClaimType = ClaimTypes.Role, ClaimValue = CustomRoles.User };
        if (!context.UserClaims.Any())
        {
            context.Add(adminRoleClaim);
            context.Add(userRoleClaim);
            context.SaveChanges();
        }

        // Add admin && test users
        if (!context.Users.Any())
        {
            var adminUserOption = _siteSettingsOption.Value.AdminUser;
            if (adminUserOption is null)
            {
                throw new InvalidOperationException("adminUserOption is null.");
            }

            var adminUser = CreateUser(adminUserOption);
            context.Add(adminUser);
            adminUser.UserClaims.Add(adminRoleClaim);
            adminUser.UserClaims.Add(userRoleClaim);

            var testUsersOption = _siteSettingsOption.Value.TestUsers;
            if (testUsersOption is null)
            {
                throw new InvalidOperationException("testUsersOption is null.");
            }

            foreach (var user in testUsersOption)
            {
                var testUser = CreateUser(user);
                context.Add(testUser);
                testUser.UserClaims.Add(userRoleClaim);
            }

            context.SaveChanges();
        }
    }

    private User CreateUser(UserOptionDto user)
    {
        return new User
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            Password = _securityService.GetSha256Hash(user.Password)
        };
    }
}