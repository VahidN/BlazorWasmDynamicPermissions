using Microsoft.EntityFrameworkCore;

namespace BlazorWasmDynamicPermissions.Server.Entities;

[Index(nameof(Username), IsUnique = true)]
public class User
{
    public int Id { get; set; }

    [MaxLength(450), Required] public string Username { get; set; } = default!;

    [Required] public string Password { get; set; } = default!;

    public string? DisplayName { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<UserClaim> UserClaims { get; set; } = new HashSet<UserClaim>();

    public virtual UserToken UserToken { set; get; } = default!;
}