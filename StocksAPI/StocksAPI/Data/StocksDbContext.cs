using Microsoft.EntityFrameworkCore;
using StocksAPI.Models;

namespace StocksAPI.Data;

public class StocksDbContext(DbContextOptions<StocksDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
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
        
        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.ClientName).IsRequired().HasMaxLength(100);
            entity.Property(o => o.CreatedAt).IsRequired();
            entity.Property(o => o.CreatedBy).HasMaxLength(100);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.ProductId).IsRequired();
            entity.Property(oi => oi.ColorId).IsRequired();
            entity.Property(oi => oi.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(oi => oi.ColorCode).IsRequired();
            entity.Property(oi => oi.Quantity).IsRequired();
            
            // Configure relationship
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
