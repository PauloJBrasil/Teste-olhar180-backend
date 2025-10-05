using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Status).IsRequired().HasMaxLength(50);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // Índice para acelerar filtros por proprietário
            entity.HasIndex(t => t.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Phone).IsRequired().HasMaxLength(20);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}