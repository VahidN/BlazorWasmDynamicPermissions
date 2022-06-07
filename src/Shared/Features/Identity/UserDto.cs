namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

public class UserDto
{
    public int Id { get; set; }

    public string Username { get; set; } = default!;

    public string? DisplayName { get; set; }
}