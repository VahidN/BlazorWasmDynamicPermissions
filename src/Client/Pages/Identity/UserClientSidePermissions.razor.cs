using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.Pages.Identity;

[Authorize(Roles = CustomRoles.Admin)]
public partial class UserClientSidePermissions
{
    [Parameter] public int UserId { set; get; }

    private HashSet<string> SelectedUrls { get; set; } = new(StringComparer.Ordinal);

    [Inject] private NavigationManager NavigationManager { set; get; } = default!;

    [Inject] private IHttpClientService HttpClientService { set; get; } = default!;

    [Inject] private IProtectedPagesProvider ProtectedPagesProvider { set; get; } = default!;

    private UserDto? User { set; get; }

    protected override async Task OnInitializedAsync()
    {
        var claimsResponse = await HttpClientService.GetDataAsJsonAsync<ClaimsResponseDto>(
            Invariant($"api/DynamicPermissionsManager/UserDynamicClientPermissions/{UserId}"));
        SelectedUrls = new HashSet<string>(claimsResponse?.ClaimValues ?? new List<string>(), StringComparer.Ordinal);
        User = claimsResponse?.User;
    }

    private async Task DoAddOrUpdateClaimsAsync()
    {
        var response =
            await HttpClientService.PostDataAsJsonAsync<ApiResponseDto>(
                "api/DynamicPermissionsManager/AddOrUpdateClaims",
                new DynamicClaimsDto
                {
                    UserId = UserId,
                    InputClaimValues = SelectedUrls.ToList(),
                    ClaimType = CustomPolicies.DynamicClientPermission
                });
        if (response?.Success == true)
        {
            NavigationManager.NavigateTo("users-manager");
        }
    }

    private bool IsComponentChecked(string? url)
    {
        return !string.IsNullOrWhiteSpace(url) && SelectedUrls.Contains(url);
    }

    private void ComponentCheckboxClicked(string? selectedUrl, object? isChecked)
    {
        if (isChecked is null || selectedUrl is null)
        {
            return;
        }

        if ((bool)isChecked)
        {
            SelectedUrls.Add(selectedUrl);
        }
        else if (SelectedUrls.Contains(selectedUrl))
        {
            SelectedUrls.Remove(selectedUrl);
        }
    }

    private void SelectAllCheckboxClicked(object? isChecked)
    {
        foreach (var url in ProtectedPagesProvider.ProtectedPages
                     .SelectMany(groupedRoutablePage => groupedRoutablePage.GroupItems)
                     .Select(routablePage => routablePage.Url))
        {
            ComponentCheckboxClicked(url, isChecked);
        }
    }
}