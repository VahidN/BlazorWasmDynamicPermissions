using BlazorWasmDynamicPermissions.Shared.Features.Identity;

namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface IApiActionsDiscoveryService
{
    IReadOnlyList<ApiControllerDto> DynamicallySecuredActions { get; }
}