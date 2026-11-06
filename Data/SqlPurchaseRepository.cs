using System.Data;
using Microsoft.Data.SqlClient;
using ShopApp.Models;
namespace ShopApp.Data;

public sealed class SqlPurchaseRepository(string connectionString) : IPurchaseRepository
{
    // Прямая модель: чтение потока строк при открытом соединении.
    public IReadOnlyList<Purchase> GetAll()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand("dbo.Purchase_List", connection) { CommandType = CommandType.StoredProcedure };
        using var reader = command.ExecuteReader();
        var result = new List<Purchase>();
        while (reader.Read()) result.Add(new Purchase(reader.GetInt32(0), reader.GetString(1),
            new Product(reader.GetString(2), reader.GetString(3), reader.GetDecimal(4)),
            new PurchaseOptions(reader.GetBoolean(5), reader.GetBoolean(6), reader.GetBoolean(7), reader.GetBoolean(8)), reader.GetDecimal(9)));
        return result;
    }
    public void Save(Purchase p, bool update)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(update ? "dbo.Purchase_Update" : "dbo.Purchase_Add", connection) { CommandType = CommandType.StoredProcedure };
        if (update) command.Parameters.Add("@Id", SqlDbType.Int).Value = p.Id;
        command.Parameters.Add("@Customer", SqlDbType.NVarChar, 80).Value = p.Customer;
        command.Parameters.Add("@Product", SqlDbType.NVarChar, 100).Value = p.Product.Name;
        command.Parameters.Add("@Category", SqlDbType.NVarChar, 40).Value = p.Product.Category;
        var price = command.Parameters.Add("@Price", SqlDbType.Decimal); price.Precision = 12; price.Scale = 2; price.Value = p.Product.Price;
        command.Parameters.Add("@Gift", SqlDbType.Bit).Value = p.Options.Gift;
        command.Parameters.Add("@Warranty", SqlDbType.Bit).Value = p.Options.Warranty;
        command.Parameters.Add("@Insurance", SqlDbType.Bit).Value = p.Options.Insurance;
        command.Parameters.Add("@Courier", SqlDbType.Bit).Value = p.Options.Courier;
        command.ExecuteNonQuery();
    }
    public void Delete(int id)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand("dbo.Purchase_Delete", connection) { CommandType = CommandType.StoredProcedure };
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.ExecuteNonQuery();
    }
    // Отсоединённая модель: адаптер закрывает соединение после Fill.
    public DataTable GetSnapshot()
    {
        using var connection = new SqlConnection(connectionString);
        using var adapter = new SqlDataAdapter("SELECT * FROM dbo.PurchaseSummary ORDER BY Id DESC", connection);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }
}
