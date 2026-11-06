using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShopApp.Models;
using ShopApp.Services;
using ShopApp.Data;
using Microsoft.Data.SqlClient;
namespace ShopApp;

public partial class MainWindow : Window
{
    private readonly IProductCatalog catalog;
    private readonly IPricingService pricing;
    private PurchaseService? purchases;
    private SqlPurchaseRepository? repository;
    private bool busy;
    private bool stale;
    private bool ready;
    public MainWindow() : this(new ProductCatalog(), new PricingService()) { }
    public MainWindow(IProductCatalog catalog, IPricingService pricing)
    {
        this.catalog = catalog;
        this.pricing = pricing;

        InitializeComponent();
        ready = true;
        CategoryBox.ItemsSource = catalog.Categories;
        CategoryBox.SelectedIndex = 0;
        Total.Content = "Нет подключения к SQL Server";
        Loaded += async (_, _) => await ConnectAsync();
    }
    private PurchaseOptions ReadOptions() => new(GiftBox.IsChecked == true, WarrantyBox.IsChecked == true,
        InsuranceBox.IsChecked == true, Courier.IsChecked == true);
    private void Category_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!ready || CategoryBox.SelectedItem is not string category) return;
        ProductsList.ItemsSource = catalog.GetProducts(category);
        ProductsList.SelectedIndex = 0;
    }
    private void Options_Changed(object sender, RoutedEventArgs e) { if (ready) UpdatePreview(); }
    private void UpdatePreview() => Preview.Content = ProductsList.SelectedItem is Product p
        ? $"Стоимость покупки: {pricing.Calculate(p, ReadOptions()):N2} ₽" : "Выберите товар";
    private void Name_Changed(object sender, TextChangedEventArgs e)
    {
        if (ready) CustomerName.ClearValue(Border.BorderBrushProperty);
    }
    private void Name_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CustomerName.Text))
        { CustomerName.BorderBrush = Brushes.Firebrick; Message("Введите имя покупателя.", true); }
    }
    private async Task RunAsync(Func<Task> operation)
    {
        if (busy) return;
        busy = true; IsEnabled = false;
        try { await operation(); }
        catch (Exception ex) when (ex is SqlException or ArgumentException or InvalidOperationException)
        { Message($"Операция не выполнена: {ex.Message}", true); }
        finally { busy = false; IsEnabled = true; }
    }
    private async Task ConnectAsync() => await RunAsync(async () =>
    {
        var candidate = new SqlPurchaseRepository(ConnectionBox.Text.Trim());
        var service = new PurchaseService(pricing, candidate);
        var items = await Task.Run(service.GetAll);
        repository = candidate; purchases = service; stale = false;
        Display(items);
        Message("Подключено к SQL Server. Покупки загружены.");
    });
    private async void Connect_Click(object sender, RoutedEventArgs e) => await ConnectAsync();
    private async Task ReloadAsync()
    {
        if (purchases is null) return;
        Display(await Task.Run(purchases.GetAll));
        stale = false;
    }
    private void Display(IReadOnlyList<Purchase> items)
    {
        Cart.ItemsSource = items;
        Total.Content = $"Позиций: {items.Count} · Итого: {items.Sum(p => p.Total):N2} ₽";
        UpdatePreview();
    }
    private bool CanWrite()
    {
        if (purchases is not null && !stale) return true;
        Message("Подключитесь к базе и обновите список перед изменением.", true);
        return false;
    }
    private async Task WriteAsync(Action action) => await RunAsync(async () =>
    {
        await Task.Run(action);
        stale = true;
        try { await ReloadAsync(); Message("Изменения сохранены в SQL Server. Список обновлён."); }
        catch (SqlException) { Message("Изменения уже сохранены, но чтение не удалось. Обновите список перед следующей записью.", true); }
    });
    private async Task SaveAsync(bool update)
    {
        if (!CanWrite()) return;
        if (update && Cart.SelectedItem is not Purchase) return;
        if (ProductsList.SelectedItem is not Product product) { Message("Выберите товар.", true); return; }
        if (string.IsNullOrWhiteSpace(CustomerName.Text))
        { CustomerName.BorderBrush = Brushes.Firebrick; CustomerName.Focus(); Message("Введите имя покупателя.", true); return; }
        string customer = CustomerName.Text;
        var options = ReadOptions();
        int? id = update ? (Cart.SelectedItem as Purchase)?.Id : null;
        await WriteAsync(() => purchases!.Save(customer, product, options, id));
    }
    private async void Add_Click(object sender, RoutedEventArgs e) => await SaveAsync(false);
    private async void Update_Click(object sender, RoutedEventArgs e) => await SaveAsync(true);
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await ConnectAsync();
    private async void Snapshot_Click(object sender, RoutedEventArgs e)
    {
        if (repository is null) { Message("Сначала подключитесь к базе данных.", true); return; }
        await RunAsync(async () =>
        {
            var table = await Task.Run(repository.GetSnapshot);
            new Window { Owner = this, Title = "Отсоединённая модель: снимок DataTable", Width = 1050, Height = 500,
                Content = new DataGrid { ItemsSource = table.DefaultView, IsReadOnly = true, AutoGenerateColumns = true } }.Show();
            Message("Снимок загружен. Соединение закрыто; таблица доступна в памяти.");
        });
    }
    private void Cart_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!ready) return;
        UpdateButton.IsEnabled = DeleteButton.IsEnabled = Cart.SelectedItem is Purchase;
        if (Cart.SelectedItem is not Purchase p) { Details.Text = "Выберите покупку для просмотра или изменения."; return; }
        CustomerName.Text = p.Customer;
        CategoryBox.SelectedItem = p.Product.Category;
        ProductsList.SelectedItem = p.Product;
        GiftBox.IsChecked = p.Options.Gift; WarrantyBox.IsChecked = p.Options.Warranty; InsuranceBox.IsChecked = p.Options.Insurance;
        Courier.IsChecked = p.Options.Courier; Pickup.IsChecked = !p.Options.Courier;
        Details.Text = p.Details;
        UpdatePreview();
    }
    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (!CanWrite() || Cart.SelectedItem is not Purchase p) return;
        if (MessageBox.Show($"Удалить покупку №{p.Id}?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        await WriteAsync(() => purchases!.Remove(p.Id));
    }
    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        Cart.SelectedItem = null;
        CustomerName.Clear();
        CustomerName.ClearValue(Border.BorderBrushProperty);
        GiftBox.IsChecked = WarrantyBox.IsChecked = InsuranceBox.IsChecked = false;
        Pickup.IsChecked = true;
        CategoryBox.SelectedIndex = 0;
        ProductsList.SelectedIndex = 0;
        UpdatePreview();
        Message("Форма очищена.");
    }
    private void Message(string text, bool error = false)
    {
        Status.Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        Status.Foreground = error ? Brushes.Firebrick : new SolidColorBrush(Color.FromRgb(22, 117, 74));
        StatusPanel.Background = error ? Brushes.MistyRose : new SolidColorBrush(Color.FromRgb(234, 246, 239));
    }
}
