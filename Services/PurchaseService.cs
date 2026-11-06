using ShopApp.Data;
using ShopApp.Models;
namespace ShopApp.Services;
public sealed class PurchaseService(IPricingService pricing, IPurchaseRepository repository)
{
    public IReadOnlyList<Purchase> GetAll() => repository.GetAll();
    public void Save(string customer, Product product, PurchaseOptions options, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(customer) || customer.Trim().Length > 80)
            throw new ArgumentException("Введите имя покупателя (от 1 до 80 символов).");
        repository.Save(new Purchase(id ?? 0, customer.Trim(), product, options, pricing.Calculate(product, options)), id.HasValue);
    }
    public void Remove(int id) => repository.Delete(id);
}
