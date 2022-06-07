namespace BlazorWasmDynamicPermissions.Server.Models.SiteOptions;

public class UserOptionDto
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
}