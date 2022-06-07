namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "(*)")] public string OldPassword { get; set; } = default!;

    [Required(ErrorMessage = "(*)")] public string NewPassword { get; set; } = default!;

    [Required(ErrorMessage = "(*)"), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = default!;
}