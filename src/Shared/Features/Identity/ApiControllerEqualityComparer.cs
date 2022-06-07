namespace BlazorWasmDynamicPermissions.Shared.Features.Identity;

/// <summary>
///     A Controller Dto IEqualityComparer
/// </summary>
public class ApiControllerEqualityComparer : IEqualityComparer<ApiControllerDto>
{
    public bool Equals(ApiControllerDto? x, ApiControllerDto? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return string.Equals(x.AreaName, y.AreaName, StringComparison.Ordinal) &&
               string.Equals(x.ControllerName, y.ControllerName, StringComparison.Ordinal);
    }

    public int GetHashCode(ApiControllerDto obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        return HashCode.Combine(obj.AreaName, obj.ControllerName);
    }
}