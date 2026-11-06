using Microsoft.EntityFrameworkCore;
using ShopApp.Data.Entities;
using ShopApp.Models;
using ShopApp.Services;
namespace ShopApp.Data;

public sealed class EfPurchaseRepository(string connectionString, IPricingService pricing) : IPurchaseRepository
{
    private AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString).Options);

    // Учебный Code First: применяем версионируемые миграции перед первой загрузкой.
    public void Initialize()
    {
        using var db = CreateContext();
        db.Database.Migrate();
    }
    public IReadOnlyList<Purchase> GetAll()
    {
        using var db = CreateContext();
        // LINQ переводится ORM в SELECT ... ORDER BY; отслеживание для чтения отключено.
        return db.Purchases.AsNoTracking().OrderByDescending(p => p.Id)
            .ToList().Select(ToModel).ToArray();
    }
    public void Save(Purchase purchase, bool update)
    {
        using var db = CreateContext();
        var entity = update
            ? db.Purchases.SingleOrDefault(p => p.Id == purchase.Id)
                ?? throw new InvalidOperationException("Покупка уже удалена. Обновите список.")
            : new PurchaseEntity();
        entity.Customer = purchase.Customer;
        entity.Product = purchase.Product.Name;
        entity.Category = purchase.Product.Category;
        entity.Price = purchase.Product.Price;
        entity.Gift = purchase.Options.Gift;
        entity.Warranty = purchase.Options.Warranty;
        entity.Insurance = purchase.Options.Insurance;
        entity.Courier = purchase.Options.Courier;
        if (!update) db.Purchases.Add(entity);
        // Change Tracker определяет INSERT или UPDATE и формирует параметры запроса.
        db.SaveChanges();
    }
    public void Delete(int id)
    {
        using var db = CreateContext();
        var entity = db.Purchases.SingleOrDefault(p => p.Id == id)
            ?? throw new InvalidOperationException("Покупка уже удалена. Обновите список.");
        db.Purchases.Remove(entity);
        db.SaveChanges();
    }
    private Purchase ToModel(PurchaseEntity entity)
    {
        var product = new Product(entity.Product, entity.Category, entity.Price);
        var options = new PurchaseOptions(entity.Gift, entity.Warranty, entity.Insurance, entity.Courier);
        return new Purchase(entity.Id, entity.Customer, product, options, pricing.Calculate(product, options));
    }
}
