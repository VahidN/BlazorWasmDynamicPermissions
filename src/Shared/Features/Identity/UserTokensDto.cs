namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

public class UserTokensDto
{
    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public string? DynamicPermissionsToken { set; get; }
}