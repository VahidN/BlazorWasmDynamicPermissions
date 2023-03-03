using BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services.Contracts;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

/// <summary>
///     A helper to find all of the routable components
///     which have a ProtectedPageAttribute.
/// </summary>
public class ProtectedPagesProvider : IProtectedPagesProvider
{
    public ProtectedPagesProvider()
    {
        ProtectedPages = GetProtectedPages();
    }

    /// <summary>
    ///     The list of all routable components which have a RoutablePageAttribute.
    /// </summary>
    public IReadOnlyList<GroupedProtectedPage> ProtectedPages { get; }

    private static List<GroupedProtectedPage> GetProtectedPages()
    {
        var allComponents = Assembly
            .GetExecutingAssembly()
            .ExportedTypes
            .Where(t => typeof(IComponent).IsAssignableFrom(t));

        var results = new List<ProtectedPageAttribute>();

        foreach (var component in allComponents)
        {
            var allAttributes = component.GetCustomAttributes(true);

            var routeAttribute = allAttributes.OfType<RouteAttribute>().FirstOrDefault();
            if (routeAttribute is null)
            {
                // routable components only
                continue;
            }

            var protectedPageAttribute = allAttributes.OfType<ProtectedPageAttribute>().FirstOrDefault();
            if (protectedPageAttribute is null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(protectedPageAttribute.Url))
            {
                protectedPageAttribute.Url = routeAttribute.Template.NormalizeRelativePath();
            }

            results.Add(protectedPageAttribute);
        }

        return results
            .OrderBy(protectedPageAttribute => protectedPageAttribute.GroupOrder)
            .ThenBy(protectedPageAttribute => protectedPageAttribute.ItemOrder)
            .GroupBy(protectedPageAttribute => protectedPageAttribute.GroupName)
            .Select(grouping =>
                new GroupedProtectedPage
                {
                    GroupName = grouping.Key,
                    GroupItems = grouping.Select(attribute => attribute).ToList()
                })
            .ToList();
    }
}