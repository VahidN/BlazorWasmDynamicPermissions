namespace BlazorWasmDynamicPermissions.Server.Models.SiteOptions;

public class BearerTokenOptionDto
{
    public string Key { set; get; } = default!;
    public string Issuer { set; get; } = default!;
    public string Audience { set; get; } = default!;
    public int AccessTokenExpirationMinutes { set; get; }
    public int RefreshTokenExpirationMinutes { set; get; }
}