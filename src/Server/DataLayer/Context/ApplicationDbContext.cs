using BlazorWasmDynamicPermissions.Server.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorWasmDynamicPermissions.Server.DataLayer.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<User> Users { set; get; } = default!;
    public virtual DbSet<UserClaim> UserClaims { set; get; } = default!;
    public virtual DbSet<UserToken> UserTokens { set; get; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        // it should be placed here, otherwise it will rewrite the following settings!
        base.OnModelCreating(modelBuilder);

        // Custom application mappings

        SetCaseInsensitiveSearchesForSQLite(modelBuilder);
    }

    private void SetCaseInsensitiveSearchesForSQLite(ModelBuilder modelBuilder)
    {
        if (modelBuilder == null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        if (!Database.IsSqlite())
        {
            return;
        }

        modelBuilder.UseCollation("NOCASE");

        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(string)))
        {
            property.SetCollation("NOCASE");
        }
    }
}