using Microsoft.EntityFrameworkCore;
using ShopApp.Data.Entities;
namespace ShopApp.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PurchaseEntity> Purchases => Set<PurchaseEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var purchase = modelBuilder.Entity<PurchaseEntity>();
        purchase.ToTable("Purchases", table =>
        {
            table.HasCheckConstraint("CK_Purchases_Customer", "LEN(LTRIM(RTRIM([Customer]))) > 0");
            table.HasCheckConstraint("CK_Purchases_Price", "[Price] > 0");
        });
        purchase.HasKey(p => p.Id);
        purchase.Property(p => p.Customer).HasMaxLength(80).IsRequired();
        purchase.Property(p => p.Product).HasMaxLength(100).IsRequired();
        purchase.Property(p => p.Category).HasMaxLength(40).IsRequired();
        purchase.Property(p => p.Price).HasPrecision(12, 2);
        purchase.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
