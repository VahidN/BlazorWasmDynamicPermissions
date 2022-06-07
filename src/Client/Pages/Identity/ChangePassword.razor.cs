using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.Pages.Identity;

[Authorize]
public partial class ChangePassword
{
    private ChangePasswordDto Model { get; } = new();

    [Inject] private NavigationManager NavigationManager { set; get; } = default!;

    [Inject] private IHttpClientService HttpClientService { set; get; } = default!;

    private async Task DoChangePasswordAsync()
    {
        var response =
            await HttpClientService.PostDataAsJsonAsync<ApiResponseDto>("api/UsersAccountManager/ChangePassword",
                Model);
        if (response?.Success == true)
        {
            NavigationManager.NavigateTo("");
        }
    }
}