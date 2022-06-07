namespace BlazorWasmDynamicPermissions.Server.Models.SiteOptions;

public class SiteSettingsDto
{
    public BearerTokenOptionDto? BearerToken { set; get; }

    public IReadOnlyList<UserOptionDto>? TestUsers { set; get; }

    public UserOptionDto? AdminUser { set; get; }
}