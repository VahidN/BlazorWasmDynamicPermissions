namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

/// <summary>
///     Defines a group of routable pages
/// </summary>
public class GroupedProtectedPage
{
    public string? GroupName { set; get; }
    public IReadOnlyList<ProtectedPageAttribute> GroupItems { set; get; } = new List<ProtectedPageAttribute>();
}