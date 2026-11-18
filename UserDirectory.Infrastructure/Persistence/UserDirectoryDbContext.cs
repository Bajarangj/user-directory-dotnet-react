using Microsoft.EntityFrameworkCore;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Infrastructure.Persistence;

public sealed class UserDirectoryDbContext(DbContextOptions<UserDirectoryDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Name).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Age).IsRequired();
            entity.Property(user => user.City).IsRequired();
            entity.Property(user => user.State).IsRequired();
            entity.Property(user => user.Pincode).HasMaxLength(10).IsRequired();
        });
    }
}
