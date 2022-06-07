using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.Pages;

public partial class CallServerApis
{
    [Inject] private IHttpClientService HttpClientService { set; get; } = default!;

    private UserDto? UserDto { set; get; }

    private ClaimsResponseDto? ClaimsResponseDto { set; get; }

    private async Task OnCallMyUserInfoHttpGetAsync()
    {
        UserDto = null;
        UserDto = await HttpClientService.GetDataAsJsonAsync<UserDto>(
            "api/DynamicServerPermissionsSample/MyUserInfo");
    }

    private async Task OnCallMyUserInfoHttpPostAsync()
    {
        UserDto = null;
        UserDto = await HttpClientService.PostDataAsJsonAsync<UserDto>(
            "api/DynamicServerPermissionsSample/MyUserInfo");
    }

    private async Task OnCallMyDynamicClientClaimsHttpGetAsync()
    {
        ClaimsResponseDto = null;
        ClaimsResponseDto = await HttpClientService.GetDataAsJsonAsync<ClaimsResponseDto>(
            "api/DynamicServerPermissionsSample/MyDynamicClientClaims");
    }
}