using ShopApp.Models;
using ShopApp.Services;
using ShopApp.Data;
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
Console.WriteLine("All pricing, validation and repository contract checks passed.");
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
