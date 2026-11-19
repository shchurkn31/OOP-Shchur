using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine($"Логічні процесори: {Environment.ProcessorCount}");
Console.WriteLine("Операція | Елементів | LINQ, мс | PLINQ, мс | Прискорення");

var random = new Random(42);
var data = new List<int>(10_000_000);
Func<int, bool> odd = number => (number & 1) != 0;
Func<int, long> prime = number => IsPrime(number) ? 1 : 0;
Func<int, long> mix = number => Mix(number);
_ = Measure(new List<int> { 100001, 100003 }, odd, prime, false);
_ = Measure(new List<int> { 100001, 100003 }, odd, prime, true);
_ = Measure(new List<int> { 100001, 100003 }, odd, mix, false);
_ = Measure(new List<int> { 100001, 100003 }, odd, mix, true);

foreach (int size in new[] { 1_000_000, 5_000_000, 10_000_000 })
{
    while (data.Count < size)
    {
        data.Add(random.Next(100_000, 1_000_000));
    }

    Compare("Прості числа", data, odd, prime);
    Compare("Цілочисельні перетворення", data, odd, mix);
}

Console.WriteLine("\nПеревірка спільного лічильника");
const int expected = 500_000;
int workers = Math.Clamp(Environment.ProcessorCount, 2, 8);
for (int run = 1; run <= 3; run++)
{
    int unsafeCount = 0;
    int safeCount = 0;
    ParallelEnumerable.Range(0, expected).WithDegreeOfParallelism(workers)
        .ForAll(_ => unsafeCount++);
    ParallelEnumerable.Range(0, expected).WithDegreeOfParallelism(workers)
        .ForAll(_ => Interlocked.Increment(ref safeCount));
    Console.WriteLine($"Прогін {run}: без синхронізації {unsafeCount}, Interlocked {safeCount}, очікувано {expected}");
}

static void Compare(string name, List<int> data, Func<int, bool> predicate, Func<int, long> operation)
{
    var sequential = Measure(data, predicate, operation, false);
    var parallel = Measure(data, predicate, operation, true);
    if (sequential.Sum != parallel.Sum)
    {
        throw new InvalidOperationException("Результати LINQ і PLINQ відрізняються.");
    }

    Console.WriteLine(FormattableString.Invariant(
        $"{name} | {data.Count} | {sequential.Ms:F2} | {parallel.Ms:F2} | {sequential.Ms / parallel.Ms:F2}x"));
}

static (long Sum, double Ms) Measure(List<int> data, Func<int, bool> predicate,
    Func<int, long> operation, bool parallel)
{
    var timer = Stopwatch.StartNew();
    long sum = parallel
        ? data.AsParallel().Where(predicate).Select(operation).Sum()
        : data.Where(predicate).Select(operation).Sum();
    timer.Stop();
    return (sum, timer.Elapsed.TotalMilliseconds);
}

static bool IsPrime(int number)
{
    if (number < 2 || number % 2 == 0)
    {
        return number == 2;
    }

    for (int divisor = 3; divisor <= number / divisor; divisor += 2)
    {
        if (number % divisor == 0)
        {
            return false;
        }
    }

    return true;
}

static long Mix(int number)
{
    uint value = (uint)number;
    for (int step = 0; step < 32; step++)
    {
        value = unchecked(value * 1664525u + 1013904223u);
        value ^= value >> 13;
    }

    return value;
}
