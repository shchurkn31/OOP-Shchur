using System.Text;

internal class Program
{
    // Варіант 10: повторюємо тільки IOException та HttpRequestException
    private static readonly Func<Exception, bool> RetryPolicy =
        ex => ex is IOException || ex is HttpRequestException;

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var delay = TimeSpan.FromMilliseconds(200);

        // Сценарій 1: FileProcessor — 3 помилки IOException, потім успіх
        Console.WriteLine("=== Сценарій 1: читання шаблону листа (3 збої, потім успіх) ===");
        string template = string.Empty;
        try
        {
            var fileProcessor = new FileProcessor(failuresBeforeSuccess: 3);
            template = RetryHelper.ExecuteWithRetry(
                () => fileProcessor.GetEmailTemplate("templates/welcome.txt"),
                retryCount: 3,
                initialDelay: delay,
                shouldRetry: RetryPolicy);

            Console.WriteLine($"Результат: {template}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не вдалося отримати шаблон: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Сценарій 1 завершено.\n");
        }

        // Сценарій 2: NetworkClient — 2 помилки HttpRequestException, потім успіх
        Console.WriteLine("=== Сценарій 2: надсилання листа (2 збої, потім успіх) ===");
        try
        {
            var networkClient = new NetworkClient(failuresBeforeSuccess: 2);
            string body = template.Replace("{name}", "Андрію");

            RetryHelper.ExecuteWithRetry<object?>(
                () =>
                {
                    networkClient.SendEmail("student@example.com", "Ласкаво просимо", body);
                    return null;
                },
                retryCount: 3,
                initialDelay: delay,
                shouldRetry: RetryPolicy);

            Console.WriteLine("Результат: лист успішно надіслано.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не вдалося надіслати лист: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Сценарій 2 завершено.\n");
        }

        // Сценарій 3: вибіркова обробка — ArgumentException не повторюється
        Console.WriteLine("=== Сценарій 3: shouldRetry (некоректна адреса не повторюється) ===");
        try
        {
            var networkClient = new NetworkClient(failuresBeforeSuccess: 0);

            RetryHelper.ExecuteWithRetry<object?>(
                () =>
                {
                    networkClient.SendEmail("не-email", "Тест", "Текст");
                    return null;
                },
                retryCount: 3,
                initialDelay: delay,
                shouldRetry: RetryPolicy);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Перехоплено у Main: {ex.GetType().Name} — {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Сценарій 3 завершено.\n");
        }

        // Сценарій 4: вичерпання спроб (потрібно 3 повтори, дозволено лише 1)
        Console.WriteLine("=== Сценарій 4: вичерпання повторних спроб ===");
        try
        {
            var fileProcessor = new FileProcessor(failuresBeforeSuccess: 3);

            RetryHelper.ExecuteWithRetry(
                () => fileProcessor.GetEmailTemplate("templates/welcome.txt"),
                retryCount: 1,
                initialDelay: delay,
                shouldRetry: RetryPolicy);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Перехоплено у Main: {ex.GetType().Name} — {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Сценарій 4 завершено.");
        }
    }
}

public class FileProcessor
{
    private readonly int _failuresBeforeSuccess;
    private int _attempts;

    public FileProcessor(int failuresBeforeSuccess)
    {
        _failuresBeforeSuccess = failuresBeforeSuccess;
    }

    /// <summary>
    /// Імітує читання шаблону листа з файлу.
    /// Перші N викликів кидають IOException, далі повертається шаблон.
    /// </summary>
    public string GetEmailTemplate(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Шлях до шаблону не може бути порожнім.", nameof(path));

        _attempts++;

        if (_attempts <= _failuresBeforeSuccess)
            throw new IOException($"Тимчасова помилка читання файлу '{path}' (виклик №{_attempts}).");

        return "Вітаємо, {name}! Це тестовий лист, надісланий із шаблону.";
    }
}

public class NetworkClient
{
    private readonly int _failuresBeforeSuccess;
    private int _attempts;

    public NetworkClient(int failuresBeforeSuccess)
    {
        _failuresBeforeSuccess = failuresBeforeSuccess;
    }

    /// <summary>
    /// Імітує надсилання листа.
    /// Перші N викликів кидають HttpRequestException, далі лист "надсилається".
    /// </summary>
    public void SendEmail(string recipient, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(recipient) || !recipient.Contains('@'))
            throw new ArgumentException($"Некоректна адреса отримувача: '{recipient}'.", nameof(recipient));

        _attempts++;

        if (_attempts <= _failuresBeforeSuccess)
            throw new HttpRequestException($"Поштовий сервер недоступний (виклик №{_attempts}).");

        Console.WriteLine($"  [SMTP] Лист надіслано на {recipient}. Тема: \"{subject}\". Текст: \"{body}\"");
    }
}

public static class RetryHelper
{
    /// <summary>
    /// Виконує операцію з повторними спробами та експоненційною затримкою.
    /// Затримка перед повтором N (N = 0, 1, 2...) = initialDelay * 2^N.
    /// </summary>
    public static T ExecuteWithRetry<T>(
        Func<T> operation,
        int retryCount = 3,
        TimeSpan initialDelay = default,
        Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        if (initialDelay == default)
            initialDelay = TimeSpan.FromMilliseconds(200);

        shouldRetry ??= _ => true;

        int retryAttempt = 0;

        while (true)
        {
            Console.WriteLine($"  [Retry] Спроба {retryAttempt + 1} з {retryCount + 1}...");

            try
            {
                T result = operation();
                Console.WriteLine($"  [Retry] Спроба {retryAttempt + 1}: успіх.");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"  [Retry] Спроба {retryAttempt + 1}: помилка {ex.GetType().Name} — {ex.Message}");

                if (!shouldRetry(ex))
                {
                    Console.WriteLine("  [Retry] Цей тип помилки не підлягає повтору. Зупинка.");
                    throw;
                }

                if (retryAttempt >= retryCount)
                {
                    Console.WriteLine("  [Retry] Вичерпано всі повторні спроби. Зупинка.");
                    throw;
                }

                var delay = TimeSpan.FromMilliseconds(
                    initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt));

                Console.WriteLine($"  [Retry] Очікування {delay.TotalMilliseconds:0} мс перед наступною спробою.");
                Thread.Sleep(delay);

                retryAttempt++;
            }
        }
    }
}