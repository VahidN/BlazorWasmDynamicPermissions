namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

public class UserLoginDto
{
    [Required(ErrorMessage = "(*)")] public string Username { get; set; } = default!;

    [Required(ErrorMessage = "(*)")] public string Password { get; set; } = default!;
}