using Microsoft.EntityFrameworkCore;
namespace ShopApp.Data.Migrations;

// Не изменять: снимок модели начальной миграции.
internal static class InitialModel
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.22");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 128);
        modelBuilder.UseIdentityColumns();
        modelBuilder.Entity("ShopApp.Data.Entities.PurchaseEntity", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            b.Property<string>("Customer").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<string>("Product").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("Category").IsRequired().HasMaxLength(40).HasColumnType("nvarchar(40)");
            b.Property<decimal>("Price").HasPrecision(12, 2).HasColumnType("decimal(12,2)");
            b.Property<bool>("Gift").HasColumnType("bit");
            b.Property<bool>("Warranty").HasColumnType("bit");
            b.Property<bool>("Insurance").HasColumnType("bit");
            b.Property<bool>("Courier").HasColumnType("bit");
            b.Property<DateTime>("CreatedAt").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            b.HasKey("Id");
            b.ToTable("Purchases", t =>
            {
                t.HasCheckConstraint("CK_Purchases_Customer", "LEN(LTRIM(RTRIM([Customer]))) > 0");
                t.HasCheckConstraint("CK_Purchases_Price", "[Price] > 0");
            });
        });
    }
}
