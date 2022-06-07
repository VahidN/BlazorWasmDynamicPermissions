namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

/// <summary>
///     A Controller Dto
/// </summary>
public class ApiControllerDto
{
    /// <summary>
    ///     Return `AreaAttribute.RouteValue`
    /// </summary>
    public string AreaName { get; set; } = default!;

    /// <summary>
    ///     Returns the `DisplayNameAttribute` value
    /// </summary>
    public string ControllerDisplayName { get; set; } = default!;

    /// <summary>
    ///     Return ControllerActionDescriptor.ControllerName
    /// </summary>
    public string ControllerName { get; set; } = default!;

    /// <summary>
    ///     Returns the list of the Controller's action methods.
    /// </summary>
    public IList<ApiActionDto> ApiActions { get; set; } = new List<ApiActionDto>();
}