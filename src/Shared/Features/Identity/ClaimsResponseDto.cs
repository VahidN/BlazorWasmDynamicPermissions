namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

public class ClaimsResponseDto
{
    public UserDto? User { set; get; }

    public IReadOnlyList<string> ClaimValues { set; get; } = new List<string>();
}