using ShopApp.Models;
namespace ShopApp.Services;
public interface IPricingService { decimal Calculate(Product product, PurchaseOptions options); }
public sealed class PricingService : IPricingService
{
    public decimal Calculate(Product product, PurchaseOptions options) => product.Price
        + (options.Gift ? 200m : 0m) + (options.Warranty ? 1000m : 0m)
        + (options.Insurance ? decimal.Round(product.Price * 0.03m, 2, MidpointRounding.AwayFromZero) : 0m)
        + (options.Courier ? 350m : 0m);
}
