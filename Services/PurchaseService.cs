using ShopApp.Models;
namespace ShopApp.Services;
public sealed class PurchaseService(IPricingService pricing)
{
    private readonly List<Purchase> purchases = [];
    private int nextId = 1;
    public IReadOnlyList<Purchase> Items => purchases.AsReadOnly();
    public Purchase Save(string customer, Product product, PurchaseOptions options, int? id = null)
    {
        if (string.IsNullOrWhiteSpace(customer) || customer.Trim().Length > 80)
            throw new ArgumentException("Введите имя покупателя (от 1 до 80 символов).");
        int index = id.HasValue ? purchases.FindIndex(p => p.Id == id.Value) : -1;
        if (id.HasValue && index < 0) throw new ArgumentException("Выбранная покупка не найдена.");
        var purchase = new Purchase(id ?? nextId++, customer.Trim(), product, options, pricing.Calculate(product, options));
        if (index >= 0) purchases[index] = purchase; else purchases.Add(purchase);
        return purchase;
    }
    public void Remove(int id) => purchases.RemoveAll(p => p.Id == id);
    public void Clear() => purchases.Clear();
}
