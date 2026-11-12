using System.Text;

internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var service = new InventoryService();

        service.AddStock(1, 10);
        service.AddStock(2, 5);

        Console.WriteLine("=== Частина 1: DecreaseStock (власний виняток) ===");

        try
        {
            service.DecreaseStock(1, 4);
            Console.WriteLine("Товар №1: списано 4 одиниці успішно.");

            service.DecreaseStock(2, 8);
            Console.WriteLine("Цей рядок не буде виведено.");
        }
        catch (ProductOutOfStockException ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
            Console.WriteLine($"  ProductId = {ex.ProductId}, RequestedQuantity = {ex.RequestedQuantity}, " +
                              $"AvailableQuantity = {ex.AvailableQuantity}");
        }
        finally
        {
            Console.WriteLine("Блок finally: обробку списання завершено.\n");
        }

        try
        {
            service.DecreaseStock(999, 1);
        }
        catch (ProductOutOfStockException ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}\n");
        }

        try
        {
            service.DecreaseStock(1, 0);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Некоректні вхідні дані: {ex.Message}\n");
        }

        Console.WriteLine("=== Частина 2: AddStock (патерн Result) ===");

        PrintResult("AddStock(1, 15)", service.AddStock(1, 15));
        PrintResult("AddStock(3, 7)", service.AddStock(3, 7));
        PrintResult("AddStock(1, 0)", service.AddStock(1, 0));
        PrintResult("AddStock(2, -5)", service.AddStock(2, -5));

        Console.WriteLine("\n=== Частина 3: GetStock (Result<int>) ===");

        foreach (int id in new[] { 1, 2, 3, 999 })
        {
            Result<int> stockResult = service.GetStock(id);

            if (stockResult.IsSuccess)
                Console.WriteLine($"Товар №{id}: на складі {stockResult.Value} од.");
            else
                Console.WriteLine($"Товар №{id}: помилка — {stockResult.Error}");
        }

        Console.WriteLine("\n=== Частина 4: Result.Failure(Exception) ===");

        Result<bool> wrapped = TryDecrease(service, 2, 100);
        PrintResult("TryDecrease(2, 100)", wrapped);
    }

    private static Result<bool> TryDecrease(InventoryService service, int productId, int quantity)
    {
        try
        {
            service.DecreaseStock(productId, quantity);
            return Result<bool>.Success(true);
        }
        catch (ProductOutOfStockException ex)
        {
            return Result<bool>.Failure(ex);
        }
    }

    private static void PrintResult(string operation, Result<bool> result)
    {
        if (result.IsSuccess)
            Console.WriteLine($"{operation}: успіх, Value = {result.Value}");
        else
            Console.WriteLine($"{operation}: помилка — {result.Error}");
    }
}

public class ProductOutOfStockException : Exception
{
    public int ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public ProductOutOfStockException()
    {
    }

    public ProductOutOfStockException(string message) : base(message)
    {
    }

    public ProductOutOfStockException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ProductOutOfStockException(int productId, int requestedQuantity, int availableQuantity = 0)
        : base($"Недостатньо товару №{productId} на складі: запитано {requestedQuantity}, " +
               $"доступно {availableQuantity}.")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }

    private Result(T? value, bool isSuccess, string error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, true, string.Empty);

    public static Result<T> Failure(string error) => new(default, false, error);

    public static Result<T> Failure(Exception ex) => new(default, false, ex.Message);
}

public class InventoryService
{
    private readonly Dictionary<int, int> _stock = new();

    public void DecreaseStock(int productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Кількість має бути більшою за 0.");

        _stock.TryGetValue(productId, out int available);

        if (available < quantity)
            throw new ProductOutOfStockException(productId, quantity, available);

        _stock[productId] = available - quantity;
    }

    public Result<bool> AddStock(int productId, int quantity)
    {
        if (quantity <= 0)
            return Result<bool>.Failure($"Кількість має бути більшою за 0 (отримано {quantity}).");

        _stock.TryGetValue(productId, out int current);
        _stock[productId] = current + quantity;

        return Result<bool>.Success(true);
    }

    public Result<int> GetStock(int productId)
    {
        if (!_stock.TryGetValue(productId, out int quantity))
            return Result<int>.Failure($"Товар №{productId} не знайдено.");

        return Result<int>.Success(quantity);
    }
}