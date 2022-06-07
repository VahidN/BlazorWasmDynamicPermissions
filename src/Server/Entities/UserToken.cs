using Microsoft.EntityFrameworkCore;

namespace BlazorWasmDynamicPermissions.Server.Entities;

[Index(nameof(AccessTokenSerialNumber), IsUnique = true),
 Index(nameof(RefreshTokenSerialNumber), IsUnique = true)]
public class UserToken
{
    public int Id { get; set; }

    [MaxLength(450), Required] public string AccessTokenSerialNumber { get; set; } = default!;

    [MaxLength(450), Required] public string RefreshTokenSerialNumber { get; set; } = default!;

    public int UserId { get; set; }
    public virtual User User { get; set; } = default!;
}