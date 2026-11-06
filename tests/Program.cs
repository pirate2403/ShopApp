using ShopApp.Models;
using ShopApp.Services;
var pricing = new PricingService();
var service = new PurchaseService(pricing);
var product = new Product("Футболка", "Одежда", 1200m);
var options = new PurchaseOptions(true, true, true, true);
void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
Check(pricing.Calculate(product, options) == 2786m, "Цена с услугами и доставкой");
Check(pricing.Calculate(product, new(false, false, false, false)) == 1200m, "Базовая цена");
try { service.Save("   ", product, options); throw new Exception("Пустое имя принято"); } catch (ArgumentException) { }
Check(service.Items.Count == 0, "Ошибка ввода меняет корзину");
var first = service.Save(" Анна ", product, options);
Check(first.Customer == "Анна", "Удаление пробелов");
var changed = service.Save("Иван", product, new(false, false, false, false), first.Id);
Check(service.Items.Count == 1 && changed.Id == first.Id && changed.Total == 1200m, "Изменение записи");
service.Remove(first.Id);
Check(service.Items.Count == 0, "Удаление");
service.Save("Анна", product, options); service.Clear();
Check(service.Items.Count == 0, "Очистка");
Console.WriteLine("Все проверки расчётов, валидации и корзины пройдены.");
