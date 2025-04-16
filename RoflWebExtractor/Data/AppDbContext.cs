using Microsoft.EntityFrameworkCore;
using RoflWebExtractor.Models;

namespace RoflWebExtractor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ConvertedFile> ConvertedFiles { get; set; }
    
    public DbSet<MatchStats> MatchStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<ConvertedFile>()
            .HasIndex(f => f.CreatedAt);
        
        modelBuilder.Entity<MatchStats>()
            .HasIndex(f => f.CreatedAt);
    }
} 