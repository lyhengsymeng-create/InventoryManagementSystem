
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.SKU)
                .IsUnique();

            entity.ToTable("Products", table =>
            {

                table.HasCheckConstraint(
                    "CK_Products_Name_NotEmpty",
                    "length(trim(\"Name\")) > 0");

                table.HasCheckConstraint(
                    "CK_Products_SKU_NotEmpty",
                    "length(trim(\"SKU\")) > 0");

                table.HasCheckConstraint(
                    "CK_Products_UnitPrice_NonNegative",
                    "\"UnitPrice\" >= 0");

                table.HasCheckConstraint(
                    "CK_Products_Quantity_NonNegative",
                    "\"Quantity\" >= 0");

            });

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // Map Category to the existing lowercase PostgreSQL table.
        modelBuilder.Entity<Category>()
                .ToTable("categories");

    }

}
