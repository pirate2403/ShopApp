using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ShopApp.Models;
using ShopApp.Services;
namespace ShopApp;

public partial class MainWindow : Window
{
    private readonly IProductCatalog catalog;
    private readonly IPricingService pricing;
    private readonly PurchaseService purchases;
    private bool ready;
    public MainWindow() : this(new ProductCatalog(), new PricingService()) { }
    public MainWindow(IProductCatalog catalog, IPricingService pricing)
    {
        this.catalog = catalog;
        this.pricing = pricing;
        purchases = new PurchaseService(pricing);
        InitializeComponent();
        ready = true;
        CategoryBox.ItemsSource = catalog.Categories;
        CategoryBox.SelectedIndex = 0;
        RefreshCart();
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
    private void Save(bool update)
    {
        if (update && Cart.SelectedItem is not Purchase) return;
        if (ProductsList.SelectedItem is not Product product) { Message("Выберите товар.", true); return; }
        try
        {
            // Сервис проверяет ввод и рассчитывает стоимость независимо от UI.
            var selected = Cart.SelectedItem as Purchase;
            var result = purchases.Save(CustomerName.Text, product, ReadOptions(), update ? selected?.Id : null);
            RefreshCart();
            Cart.SelectedItem = result;
            Message(update ? "Покупка изменена." : "Покупка добавлена в корзину.");
        }
        catch (ArgumentException ex)
        { CustomerName.BorderBrush = Brushes.Firebrick; CustomerName.Focus(); Message(ex.Message, true); }
    }
    private void Add_Click(object sender, RoutedEventArgs e) => Save(false);
    private void Update_Click(object sender, RoutedEventArgs e) => Save(true);
    private void Refresh_Click(object sender, RoutedEventArgs e) { RefreshCart(); Message("Список покупок обновлён."); }
    private void RefreshCart()
    {
        Cart.ItemsSource = purchases.Items.ToArray();
        Total.Content = $"Позиций: {purchases.Items.Count} · Итого: {purchases.Items.Sum(p => p.Total):N2} ₽";
        UpdatePreview();
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
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Cart.SelectedItem is not Purchase p) return;
        purchases.Remove(p.Id);
        RefreshCart();
        Message("Покупка удалена.");
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
    private void ClearCart_Click(object sender, RoutedEventArgs e)
    {
        purchases.Clear(); RefreshCart(); Message("Корзина очищена.");
    }
    private void Message(string text, bool error = false)
    {
        Status.Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
        Status.Foreground = error ? Brushes.Firebrick : new SolidColorBrush(Color.FromRgb(22, 117, 74));
        StatusPanel.Background = error ? Brushes.MistyRose : new SolidColorBrush(Color.FromRgb(234, 246, 239));
    }
}
