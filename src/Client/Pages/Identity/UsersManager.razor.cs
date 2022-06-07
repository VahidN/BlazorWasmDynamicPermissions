using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.Pages.Identity;

[Authorize(Roles = CustomRoles.Admin)]
public partial class UsersManager
{
    [Inject] private IHttpClientService HttpClientService { set; get; } = default!;

    private IReadOnlyList<UserDto>? Users { set; get; }

    protected override async Task OnInitializedAsync()
    {
        Users = await HttpClientService.GetDataAsJsonAsync<List<UserDto>>("api/UsersAccountManager/users");
    }
}