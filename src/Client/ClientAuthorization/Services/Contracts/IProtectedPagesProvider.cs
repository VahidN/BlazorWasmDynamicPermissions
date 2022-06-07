namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;

public interface IProtectedPagesProvider
{
    /// <summary>
    ///     The list of all routable components which have a RoutablePageAttribute.
    /// </summary>
    IReadOnlyList<GroupedProtectedPage> ProtectedPages { get; }
}