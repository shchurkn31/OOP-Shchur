public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _jitterFactor;

    public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor = 0.2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);
        ArgumentOutOfRangeException.ThrowIfLessThan(initialDelay, TimeSpan.Zero);
        if (!double.IsFinite(jitterFactor) || jitterFactor < 0 || jitterFactor > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(jitterFactor));
        }

        _maxRetries = maxRetries;
        _initialDelay = initialDelay;
        _jitterFactor = jitterFactor;
    }

    public T Execute<T>(Func<T> operation, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        for (int retryAttempt = 0; ; retryAttempt++)
        {
            Console.WriteLine($"Спроба {(long)retryAttempt + 1}");
            T result;
            try
            {
                result = operation();
            }
            catch (Exception error)
            {
                Console.WriteLine($"Причина невдачі: {error.GetType().Name}: {error.Message}");
                if (retryAttempt == _maxRetries || shouldRetry?.Invoke(error) == false)
                {
                    Console.WriteLine("Повтори завершено.");
                    throw;
                }

                var delay = _initialDelay == TimeSpan.Zero
                    ? TimeSpan.Zero
                    : _initialDelay * Math.Pow(2, retryAttempt)
                        * (1 + Random.Shared.NextDouble() * _jitterFactor * 2 - _jitterFactor);

                Console.WriteLine($"Затримка: {delay.TotalMilliseconds:F0} мс.");
                Thread.Sleep(delay);
                continue;
            }

            Console.WriteLine("Операція виконана.");
            return result;
        }
    }
}
