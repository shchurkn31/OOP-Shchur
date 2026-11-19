Console.OutputEncoding = System.Text.Encoding.UTF8;

var numbers = new List<int> { 2, 5, 8, 11 };
var words = new List<string> { "кіт", "папуга", "пес" };
var products = new List<Product>
{
    new("Монітор", 7200m),
    new("Миша", 650m),
    new("Клавіатура", 1800m)
};

Console.WriteLine($"Квадрати: {string.Join(", ", CollectionUtils.Map(numbers, n => n * n))}");
Console.WriteLine($"Найдовше слово: {CollectionUtils.MaxBy(words, word => word.Length)}");
Console.WriteLine("Товари від 1000 грн:");
CollectionUtils.ForEach(
    CollectionUtils.Filter(products, product => product.Price >= 1000m),
    product => Console.WriteLine($"- {product.Name}"));

public record Product(string Name, decimal Price);

public static class CollectionUtils
{
    public static IEnumerable<TResult> Map<TSource, TResult>(
        IEnumerable<TSource> collection, Func<TSource, TResult> mapper)
    {
        foreach (var item in collection)
        {
            yield return mapper(item);
        }
    }

    public static T MaxBy<T, TKey>(IEnumerable<T> collection, Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        using var iterator = collection.GetEnumerator();
        if (!iterator.MoveNext())
        {
            throw new InvalidOperationException("Колекція порожня.");
        }

        var best = iterator.Current;
        var bestKey = keySelector(best);
        while (iterator.MoveNext())
        {
            var candidate = iterator.Current;
            var key = keySelector(candidate);
            if (Comparer<TKey>.Default.Compare(key, bestKey) > 0)
            {
                best = candidate;
                bestKey = key;
            }
        }

        return best;
    }

    public static IEnumerable<T> Filter<T>(IEnumerable<T> collection, Predicate<T> predicate)
    {
        foreach (var item in collection)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    public static void ForEach<T>(IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }
}
