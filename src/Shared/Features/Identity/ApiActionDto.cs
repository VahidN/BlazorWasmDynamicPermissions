namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

/// <summary>
///     An Action Dto
/// </summary>
public class ApiActionDto
{
    /// <summary>
    ///     Returns `DisplayNameAttribute` value of the action method.
    /// </summary>
    public string ActionDisplayName { get; set; } = default!;

    /// <summary>
    ///     Return ControllerActionDescriptor.ActionName
    /// </summary>
    public string ActionName { get; set; } = default!;

    /// <summary>
    ///     The action method's HTTP method
    /// </summary>
    public IEnumerable<string> HttpMethods { set; get; } = new List<string>();
}