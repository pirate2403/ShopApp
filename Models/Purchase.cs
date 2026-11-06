namespace ShopApp.Models;
public sealed record Product(string Name, string Category, decimal Price)
{
    public override string ToString() => $"{Name} — {Price:N0} ₽";
}
public sealed record PurchaseOptions(bool Gift, bool Warranty, bool Insurance, bool Courier);
public sealed record Purchase(int Id, string Customer, Product Product, PurchaseOptions Options, decimal Total)
{
    public string Amount => $"{Total:N2} ₽";
    public string Details => $"Покупка №{Id}\n{Customer} · {Product.Name} · {Product.Category}\n"
        + $"Упаковка: {(Options.Gift ? "да" : "нет")}; гарантия: {(Options.Warranty ? "да" : "нет")}; страховка: {(Options.Insurance ? "да" : "нет")}\n"
        + (Options.Courier ? "Курьерская доставка" : "Самовывоз");
}
