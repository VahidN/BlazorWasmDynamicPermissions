using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.Pages.Identity;

[Authorize(Roles = CustomRoles.Admin)]
public partial class UserServerSidePermissions
{
    [Parameter] public int UserId { set; get; }

    private HashSet<string> SelectedActionIds { get; set; } = new(StringComparer.Ordinal);

    [Inject] private NavigationManager NavigationManager { set; get; } = default!;

    [Inject] private IHttpClientService HttpClientService { set; get; } = default!;

    private IReadOnlyList<ApiControllerDto>? DynamicallySecuredActions { set; get; }

    private UserDto? User { set; get; }

    protected override async Task OnInitializedAsync()
    {
        var response = await HttpClientService.GetDataAsJsonAsync<SecuredActionsDto>(
            Invariant($"api/DynamicPermissionsManager/DynamicallySecuredServerActions/{UserId}"));
        DynamicallySecuredActions = response?.DynamicallySecuredActions;
        User = response?.UserClaims?.User;
        SelectedActionIds = new HashSet<string>(response?.UserClaims?.ClaimValues ?? new List<string>(),
            StringComparer.Ordinal);
    }

    private async Task DoAddOrUpdateClaimsAsync()
    {
        var response =
            await HttpClientService.PostDataAsJsonAsync<ApiResponseDto>(
                "api/DynamicPermissionsManager/AddOrUpdateClaims",
                new DynamicClaimsDto
                {
                    UserId = UserId,
                    InputClaimValues = SelectedActionIds.ToList(),
                    ClaimType = CustomPolicies.DynamicServerPermission
                });
        if (response?.Success == true)
        {
            NavigationManager.NavigateTo("users-manager");
        }
    }

    private bool IsActionChecked(string? actionId)
    {
        return !string.IsNullOrWhiteSpace(actionId) && SelectedActionIds.Contains(actionId);
    }

    private void ActionCheckboxClicked(string? selectedActionId, object? isChecked)
    {
        if (isChecked is null || selectedActionId is null)
        {
            return;
        }

        if ((bool)isChecked)
        {
            SelectedActionIds.Add(selectedActionId);
        }
        else 
        {
            SelectedActionIds.Remove(selectedActionId);
        }
    }

    private void SelectAllCheckboxClicked(object? isChecked)
    {
        if (DynamicallySecuredActions is null)
        {
            return;
        }

        foreach (var apiController in DynamicallySecuredActions)
        {
            foreach (var apiAction in apiController.ApiActions)
            {
                var actionId = GetActionId(apiController, apiAction);
                ActionCheckboxClicked(actionId, isChecked);
            }
        }
    }

    private static string GetActionId(ApiControllerDto apiController, ApiActionDto apiAction)
    {
        return
            $"{apiController.AreaName}:{apiController.ControllerName}:{apiAction.ActionName}:{string.Join(",", apiAction.HttpMethods)}";
    }

    private static string GetHttpMethodClass(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => "bg-primary",
            "POST" => "bg-success",
            "PUT" => "bg-secondary",
            "DELETE" => "bg-danger",
            "HEAD" => "bg-warning",
            "OPTIONS" => "bg-info",
            "TRACE" => "bg-light",
            "PATCH" => "bg-dark",
            _ => "bg-secondary"
        };
    }
}