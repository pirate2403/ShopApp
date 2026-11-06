namespace ShopApp.Data.Entities;

// Снимок покупки: цена и название сохраняются независимо от текущего каталога.
public sealed class PurchaseEntity
{
    public int Id { get; set; }
    public string Customer { get; set; } = "";
    public string Product { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public bool Gift { get; set; }
    public bool Warranty { get; set; }
    public bool Insurance { get; set; }
    public bool Courier { get; set; }
    public DateTime CreatedAt { get; set; }
}
