using backend.models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class Database(DbContextOptions<Database> opts) : DbContext(opts)
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Permission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Users>()
            .HasOne(u => u.Permission)
            .WithMany(p => p.Users)
            .HasForeignKey(u => u.PermissionId);

        b.Entity<Users>()
            .HasIndex(u => u.Usuario)
            .IsUnique();
    }
}
