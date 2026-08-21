using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Models;

namespace StockFlow.Api.Data;

public sealed class StockFlowDbContext(DbContextOptions<StockFlowDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();
        product.ToTable("Products");
        product.HasKey(item => item.Id);
        product.Property(item => item.Sku).HasMaxLength(40).IsRequired();
        product.Property(item => item.Name).HasMaxLength(120).IsRequired();
        product.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
        product.HasIndex(item => item.Sku).IsUnique();
        product.HasOne(item => item.Category)
            .WithMany()
            .HasForeignKey(item => item.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        product.HasOne(item => item.Supplier)
            .WithMany()
            .HasForeignKey(item => item.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        var category = modelBuilder.Entity<Category>();
        category.ToTable("Categories");
        category.HasKey(item => item.Id);
        category.Property(item => item.Name).HasMaxLength(80).IsRequired();
        category.HasIndex(item => item.Name).IsUnique();

        var supplier = modelBuilder.Entity<Supplier>();
        supplier.ToTable("Suppliers");
        supplier.HasKey(item => item.Id);
        supplier.Property(item => item.Name).HasMaxLength(120).IsRequired();
        supplier.Property(item => item.ContactName).HasMaxLength(120);
        supplier.Property(item => item.Email).HasMaxLength(180);
        supplier.Property(item => item.Phone).HasMaxLength(30);
        supplier.HasIndex(item => item.Name).IsUnique();

        var movement = modelBuilder.Entity<StockMovement>();
        movement.ToTable("StockMovements");
        movement.HasKey(item => item.Id);
        movement.Property(item => item.Type)
            .HasConversion<string>()
            .HasMaxLength(20);
        movement.Property(item => item.Note).HasMaxLength(250);
        movement.Property(item => item.PerformedByName).HasMaxLength(120).IsRequired();
        movement.Property(item => item.PerformedByEmail).HasMaxLength(180).IsRequired();
        movement.HasOne<Product>()
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
