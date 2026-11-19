using System.Text;

public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var policy = new RetryPolicy(3, TimeSpan.FromMilliseconds(100), 0.2);

        Console.WriteLine("Зчитування залишків із тимчасово зайнятого сховища");
        int calls = 0;
        int stock = policy.Execute(() =>
        {
            if (++calls <= 2)
            {
                throw new IOException("Сховище тимчасово зайняте.");
            }

            return 27;
        }, error => error is IOException);
        Console.WriteLine($"Залишок: {stock} одиниць.");

        Console.WriteLine("\nЗчитування з постійно недоступного сховища");
        try
        {
            policy.Execute<int>(() => throw new IOException("Сховище недоступне."));
        }
        catch (IOException error)
        {
            Console.WriteLine($"Остаточний виняток: {error.Message}");
        }
    }
}
