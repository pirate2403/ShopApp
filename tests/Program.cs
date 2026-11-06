using ShopApp.Models;
using ShopApp.Services;
using ShopApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
var pricing = new PricingService();
var repository = new FakeRepository();
var service = new PurchaseService(pricing, repository);
var product = new Product("Test", "Clothes", 1200m);
var options = new PurchaseOptions(true, true, true, true);
void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
Check(pricing.Calculate(product, options) == 2786m, "Options and delivery");
Check(pricing.Calculate(product, new(false, false, false, false)) == 1200m, "Base price");
try { service.Save("   ", product, options); throw new Exception("Empty name accepted"); } catch (ArgumentException) { }
Check(service.GetAll().Count == 0, "Validation before persistence");
service.Save(" Anna ", product, options);
var first = service.GetAll().Single();
Check(first.Customer == "Anna", "Trim name");
service.Save("Ivan", product, new(false, false, false, false), first.Id);
Check(service.GetAll().Count == 1 && service.GetAll()[0].Total == 1200m, "Update");
service.Remove(first.Id);
Check(service.GetAll().Count == 0, "Delete");
// SQL Server model and migration checks without opening a network connection.
using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer("Server=localhost;Database=ShopLabOrm;Integrated Security=True").Options);
var script = db.GetService<IMigrator>().GenerateScript();
Check(script.Contains("CREATE TABLE [Purchases]") && script.Contains("CK_Purchases_Price"), "Migration DDL");
Check(script.Contains("__EFMigrationsHistory"), "Migration history");
var query = db.Purchases.AsNoTracking().OrderByDescending(p => p.Id).ToQueryString();
Check(query.Contains("ORDER BY") && query.Contains("DESC"), "LINQ translation");
var snapshot = db.GetService<IMigrationsAssembly>().ModelSnapshot!;
var snapshotModel = db.GetService<IModelRuntimeInitializer>().Initialize(snapshot.Model, designTime: true);
var changes = db.GetService<IMigrationsModelDiffer>().GetDifferences(
    snapshotModel.GetRelationalModel(), db.GetService<IDesignTimeModel>().Model.GetRelationalModel());
Check(changes.Count == 0, "Snapshot differs from current EF model");
Console.WriteLine("All pricing, validation, repository, migration and LINQ checks passed.");
sealed class FakeRepository : IPurchaseRepository
{
    private readonly List<Purchase> items = [];
    public IReadOnlyList<Purchase> GetAll() => items;
    public void Save(Purchase p, bool update)
    {
        if (update) items[items.FindIndex(x => x.Id == p.Id)] = p;
        else items.Add(p with { Id = items.Count + 1 });
    }
    public void Delete(int id) => items.RemoveAll(x => x.Id == id);
}
