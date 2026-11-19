using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var random = new Random(42);
string[] categories = ["Периферія", "Аудіо", "Мережа", "Накопичувачі"];
string[] cities = ["Рівне", "Луцьк", "Львів", "Київ"];
var products = Enumerable.Range(1, 80)
    .Select(id => new Product(id, $"Товар {id}", categories[(id - 1) % categories.Length],
        random.Next(10, 250) * 10m)).ToArray();
var customers = Enumerable.Range(1, 80)
    .Select(id => new Customer(id, $"Клієнт {id}", cities[(id - 1) % cities.Length])).ToArray();
var sales = Enumerable.Range(1, 6000)
    .Select(id => new Sale(id, random.Next(1, products.Length + 1), random.Next(1, 11),
        new DateTime(2025, 1, 1).AddDays(random.Next(730)), random.Next(1, customers.Length + 1)))
    .ToArray();

Console.WriteLine($"Продажі: {sales.Length}, товари: {products.Length}, клієнти: {customers.Length}");
Print("Найпопулярніші товари за категоріями", () => TopProductsByCategory(sales, products));
Print("Дохід за містами", () => RevenueByCity(sales, products, customers));
Print("Продажі за місяцями", () => SalesByMonth(sales, products));
Print("Топ-5 клієнтів за сумою великих покупок", () => TopCustomers(sales, products, customers));

static IEnumerable<string> TopProductsByCategory(Sale[] sales, Product[] products) => sales
    .GroupBy(sale => sale.ProductId)
    .Select(group => new { ProductId = group.Key, Units = group.Sum(sale => sale.Quantity) })
    .Join(products, summary => summary.ProductId, product => product.Id,
        (summary, product) => new { product.Category, product.Name, product.Id, summary.Units })
    .GroupBy(row => row.Category)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .SelectMany(group => group.OrderByDescending(row => row.Units).ThenBy(row => row.Id).Take(3))
    .Select(row => $"{row.Category}: {row.Name}, {row.Units} од.");

static IEnumerable<string> RevenueByCity(Sale[] sales, Product[] products, Customer[] customers) =>
    WithRevenue(sales, products)
        .Join(customers, row => row.Sale.CustomerId, customer => customer.Id,
            (row, customer) => new { customer.City, row.Revenue })
        .GroupBy(row => row.City)
        .Select(group => new { City = group.Key, Revenue = group.Sum(row => row.Revenue) })
        .OrderByDescending(row => row.Revenue)
        .Select(row => $"{row.City}: {row.Revenue:F2} грн");

static IEnumerable<string> SalesByMonth(Sale[] sales, Product[] products) =>
    WithRevenue(sales, products)
        .GroupBy(row => (row.Sale.SaleDate.Year, row.Sale.SaleDate.Month))
        .OrderBy(group => group.Key.Year)
        .ThenBy(group => group.Key.Month)
        .Select(group => $"{group.Key.Year}-{group.Key.Month:D2}: " +
            $"{group.Sum(row => row.Sale.Quantity)} од., {group.Sum(row => row.Revenue):F2} грн");

static IEnumerable<string> TopCustomers(Sale[] sales, Product[] products, Customer[] customers) =>
    WithRevenue(sales, products)
        .Where(row => row.Revenue >= 10000m)
        .GroupBy(row => row.Sale.CustomerId)
        .Select(group => new { CustomerId = group.Key, Total = group.Sum(row => row.Revenue) })
        .Join(customers, row => row.CustomerId, customer => customer.Id,
            (row, customer) => new { customer.Id, customer.Name, row.Total })
        .OrderByDescending(row => row.Total)
        .ThenBy(row => row.Id)
        .Take(5)
        .Select(row => $"{row.Name}: {row.Total:F2} грн");

static IEnumerable<(Sale Sale, decimal Revenue)> WithRevenue(Sale[] sales, Product[] products) =>
    sales.Join(products, sale => sale.ProductId, product => product.Id,
        (sale, product) => (sale, sale.Quantity * product.UnitPrice));

static void Print(string title, Func<IEnumerable<string>> query)
{
    // Прогрів перед вимірюванням і однакова матеріалізація для кожного запиту.
    _ = query().ToArray();
    var timer = Stopwatch.StartNew();
    var rows = query().ToArray();
    timer.Stop();

    Console.WriteLine(FormattableString.Invariant($"\n{title} ({timer.Elapsed.TotalMilliseconds:F3} мс)"));
    foreach (var row in rows)
    {
        Console.WriteLine(row);
    }
}

public record Sale(int Id, int ProductId, int Quantity, DateTime SaleDate, int CustomerId);
public record Product(int Id, string Name, string Category, decimal UnitPrice);
public record Customer(int Id, string Name, string City);
