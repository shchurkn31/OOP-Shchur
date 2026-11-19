using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Timeout;

Console.OutputEncoding = System.Text.Encoding.UTF8;
SendQueueMessage();
await GenerateExportAsync();
ReadCatalog();

static void SendQueueMessage()
{
    Console.WriteLine("1. Надсилання повідомлення до перевантаженої черги");
    const string messageId = "order-42";
    int attempts = 0;

    var pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<QueueBusyException>(),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(100),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                Console.WriteLine($"Повтор {args.AttemptNumber + 1} через " +
                    $"{args.RetryDelay.TotalMilliseconds:F0} мс: {args.Outcome.Exception?.Message}");
                return default;
            }
        })
        .Build();

    string receipt = pipeline.Execute(() =>
    {
        Console.WriteLine($"Спроба {++attempts}, ID {messageId}");
        if (attempts <= 2)
        {
            throw new QueueBusyException("Черга тимчасово переповнена.");
        }

        return $"Підтверджено {messageId}";
    });

    Console.WriteLine(receipt);
}

static async Task GenerateExportAsync()
{
    Console.WriteLine("\n2. Формування звіту з обмеженням часу");
    var pipeline = new ResiliencePipelineBuilder()
        .AddTimeout(new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromMilliseconds(150),
            OnTimeout = args =>
            {
                Console.WriteLine($"Перевищено ліміт {args.Timeout.TotalMilliseconds:F0} мс.");
                return default;
            }
        })
        .Build();

    try
    {
        await pipeline.ExecuteAsync(async token =>
        {
            Console.WriteLine("Розпочато формування звіту.");
            await Task.Delay(TimeSpan.FromMilliseconds(750), token);
            Console.WriteLine("Звіт сформовано.");
        });
    }
    catch (TimeoutRejectedException)
    {
        Console.WriteLine("Формування звіту скасовано за тайм-аутом.");
    }
}

static void ReadCatalog()
{
    Console.WriteLine("\n3. Читання каталогу під час тривалого збою");
    var pipeline = new ResiliencePipelineBuilder()
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<IOException>(),
            MinimumThroughput = 2,
            FailureRatio = 1.0,
            SamplingDuration = TimeSpan.FromSeconds(10),
            BreakDuration = TimeSpan.FromSeconds(2),
            OnOpened = args =>
            {
                Console.WriteLine($"Захисний вимикач відкрито на {args.BreakDuration.TotalSeconds:F0} с.");
                return default;
            }
        })
        .Build();

    for (int attempt = 1; attempt <= 3; attempt++)
    {
        try
        {
            pipeline.Execute(() =>
            {
                Console.WriteLine($"Запит {attempt} передано до каталогу.");
                throw new IOException("Сховище каталогу недоступне.");
            });
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine($"Запит {attempt} одразу відхилено захисним вимикачем.");
        }
        catch (IOException error)
        {
            Console.WriteLine(error.Message);
        }
    }
}

public class QueueBusyException(string message) : Exception(message);
