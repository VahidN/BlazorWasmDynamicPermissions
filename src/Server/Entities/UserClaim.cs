using Microsoft.EntityFrameworkCore;

namespace BlazorWasmDynamicPermissions.Server.Entities;

[Index(nameof(ClaimType), nameof(ClaimValue), IsUnique = true),
 Index(nameof(ClaimType))]
public class UserClaim
{
    public int Id { get; set; }

    [Required, MaxLength(225)] public string ClaimType { get; set; } = default!;

    [Required, MaxLength(225)] public string ClaimValue { get; set; } = default!;

    public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
}