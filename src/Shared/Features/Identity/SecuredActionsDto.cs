namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

public class SecuredActionsDto
{
    public IReadOnlyList<ApiControllerDto>? DynamicallySecuredActions { set; get; }

    public ClaimsResponseDto? UserClaims { set; get; }
}