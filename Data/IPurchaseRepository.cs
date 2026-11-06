using ShopApp.Models;
namespace ShopApp.Data;
public interface IPurchaseRepository
{
    IReadOnlyList<Purchase> GetAll();
    void Save(Purchase purchase, bool update);
    void Delete(int id);
}
