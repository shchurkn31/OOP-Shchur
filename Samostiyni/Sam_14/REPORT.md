# Звіт до самостійної роботи №14

## Сценарій 1. Тимчасово переповнена черга

Перші дві спроби повертають `QueueBusyException`. Retry обробляє саме цей виняток і відкладає повтори зі зростаючою затримкою та jitter. Черга отримує той самий ідентифікатор повідомлення. У реальній системі за цим ID потрібно усувати дублікати, якщо клієнт міг втратити підтвердження вже прийнятого повідомлення. [Документація Retry](https://www.pollydocs.org/strategies/retry.html)

```csharp
ShouldHandle = new PredicateBuilder().Handle<QueueBusyException>(),
MaxRetryAttempts = 3,
Delay = TimeSpan.FromMilliseconds(100),
BackoffType = DelayBackoffType.Exponential,
UseJitter = true,
```

Лог показує дві причини повтору й дві затримки. Третя спроба повертає підтвердження `order-42`.

## Сценарій 2. Тривале формування звіту

Імітоване формування займає 750 мс при ліміті 150 мс. Timeout передає токен скасування у функцію. Операція враховує його у `Task.Delay`. [Документація Timeout](https://www.pollydocs.org/strategies/timeout.html)

```csharp
.AddTimeout(new TimeoutStrategyOptions
{
    Timeout = TimeSpan.FromMilliseconds(150),
    OnTimeout = args =>
    {
        Console.WriteLine($"Перевищено ліміт {args.Timeout.TotalMilliseconds:F0} мс.");
        return default;
    }
})
```

Після повідомлення про перевищення часу програма перехоплює `TimeoutRejectedException` і показує, що формування скасовано. Якщо код ігнорує токен, Timeout не може примусово зупинити його роботу.

## Сценарій 3. Тривалий збій каталогу

Читання каталогу повторно завершується `IOException`. Вимикач відкривається після двох невдач за налаштованого порога 100% і тимчасово відхиляє наступні запити. Це зменшує навантаження на недоступне сховище й дозволяє швидко повернути відповідь клієнтові. [Документація Circuit Breaker](https://www.pollydocs.org/strategies/circuit-breaker.html)

```csharp
ShouldHandle = new PredicateBuilder().Handle<IOException>(),
MinimumThroughput = 2,
FailureRatio = 1.0,
SamplingDuration = TimeSpan.FromSeconds(10),
BreakDuration = TimeSpan.FromSeconds(2),
```

Лог показує два звернення до каталогу та відкриття вимикача. Третій запит отримує `BrokenCircuitException` до виклику операції.

## Висновок

Retry допомагає пережити короткочасний збій. Timeout обмежує час очікування за умови кооперативного скасування. Для серії тривалих збоїв Circuit Breaker може тимчасово припиняти виклики та зменшувати навантаження на залежність. У комбінованому пайплайні порядок стратегій визначає, чи поширюється ліміт часу на окрему спробу, чи на весь цикл повторів. Логи показують причини, затримки й результат виконання. [Документація пайплайнів Polly](https://www.pollydocs.org/pipelines/)
