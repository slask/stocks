using Microsoft.EntityFrameworkCore;
using StocksAPI.Models;

namespace StocksAPI.Data;

public class StocksDbContext(DbContextOptions<StocksDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        // Set global convention for converting enums to strings
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(50);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Category).IsRequired();

            // Configure relationship with ProductColor
            entity.HasMany(e => e.Colors)
                  .WithOne()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ProductColor entity
        modelBuilder.Entity<ProductColor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.StockCount).IsRequired().HasDefaultValue(0);
        });
    }
}
