using BlazorWasmDynamicPermissions.Shared.Features.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

/// <summary>
///     Defines a protected routable page with a default policy equal to CustomPolicies.DynamicPermission.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ProtectedPageAttribute : AuthorizeAttribute
{
    public ProtectedPageAttribute()
    {
        Policy = CustomPolicies.DynamicClientPermission;
    }

    /// <summary>
    ///     Gets or sets the route template.
    ///     This value will be filled from the RouteAttribute's template property value, if it's empty or null.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     The Group name of the current item.
    ///     It it's null, this item will become a main entry,
    ///     otherwise it will be used as the heading of the dropdown menu.
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    ///     The title of the current item.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     An optional glyph icon of the current item.
    /// </summary>
    public string? GlyphIcon { get; set; }

    /// <summary>
    ///     Oder of the current group in the final list.
    /// </summary>
    public int GroupOrder { set; get; }

    /// <summary>
    ///     Oder of the current item in the current group.
    /// </summary>
    public int ItemOrder { set; get; }
}