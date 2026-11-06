using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace ShopApp.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261106000100_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Purchases",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Customer = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Product = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                Price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                Gift = table.Column<bool>(type: "bit", nullable: false),
                Warranty = table.Column<bool>(type: "bit", nullable: false),
                Insurance = table.Column<bool>(type: "bit", nullable: false),
                Courier = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Purchases", p => p.Id);
                table.CheckConstraint("CK_Purchases_Customer", "LEN(LTRIM(RTRIM([Customer]))) > 0");
                table.CheckConstraint("CK_Purchases_Price", "[Price] > 0");
            });
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("Purchases");
    protected override void BuildTargetModel(ModelBuilder modelBuilder) => InitialModel.Build(modelBuilder);
}
