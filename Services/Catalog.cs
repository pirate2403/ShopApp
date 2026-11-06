using ShopApp.Models;
namespace ShopApp.Services;
public interface IProductCatalog
{
    IReadOnlyList<string> Categories { get; }
    IReadOnlyList<Product> GetProducts(string category);
}
public sealed class ProductCatalog : IProductCatalog
{
    private readonly Product[] products = [new("Наушники", "Электроника", 4500m), new("Смартфон", "Электроника", 29900m), new("Ноутбук", "Электроника", 69900m), new("Футболка", "Одежда", 1200m), new("Джинсы", "Одежда", 3500m), new("Куртка", "Одежда", 7900m), new("Основы C#", "Книги", 1500m), new("Алгоритмы", "Книги", 2100m)];
    public IReadOnlyList<string> Categories => products.Select(p => p.Category).Distinct().ToArray();
    public IReadOnlyList<Product> GetProducts(string category) => products.Where(p => p.Category == category).ToArray();
}
