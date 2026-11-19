using System.Diagnostics;
using System.Text.RegularExpressions;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var samples = new List<Document>
{
    new(1, "Introduction to C#", "C# is a modern object-oriented programming language developed by Microsoft."),
    new(2, "Java Basics", "Java is a popular programming language known for its portability and performance."),
    new(3, "Python Overview", "Python is a high-level programming language with simple syntax."),
    new(4, "C# Advanced Features", "C# supports LINQ, async/await, and many modern programming paradigms."),
    new(5, "Comparing Languages", "C#, Java, and Python are among the most popular programming languages today.")
};

var index = new InvertedIndex();
foreach (var document in samples)
{
    index.AddDocument(document);
}

Console.WriteLine($"Документів: {index.DocumentCount}, унікальних слів: {index.UniqueWords}");
Console.WriteLine($"AND c# async: {string.Join(", ", index.SearchMultipleWords("c#", "async"))}");
Console.WriteLine($"OR linq python: {string.Join(", ", index.SearchAnyWord("linq", "python"))}");

PerformanceTest.CompareSearchMethods(samples, "linq");
foreach (int size in new[] { 10, 100, 1000, 10000 })
{
    var documents = Enumerable.Range(1, size)
        .Select(id => samples[(id - 1) % samples.Count] with { Id = id })
        .ToList();
    PerformanceTest.CompareSearchMethods(documents, "linq");
}

if (!args.Contains("--benchmark-only"))
{
    while (true)
    {
        Console.Write("Запит AND (або exit): ");
        var query = Console.ReadLine();
        if (query is null || query.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        var results = index.SearchMultipleWords(query);
        foreach (var document in results)
        {
            Console.WriteLine($"{document}\n  {document.Content}");
        }

        Console.WriteLine($"Знайдено: {results.Count}");
    }
}

public record Document(int Id, string Title, string Content)
{
    public override string ToString() => $"[{Id}] {Title}";
}

public static class TextProcessor
{
    private static readonly Regex WordPattern = new(@"[\p{L}\p{N}]+[#]?", RegexOptions.Compiled);
    private static readonly HashSet<string> StopWords = new()
    {
        "a", "an", "the", "is", "are", "was", "were", "in", "on", "at", "to", "for", "of", "and", "or"
    };

    public static IEnumerable<string> Tokenize(string text) =>
        WordPattern.Matches(text.ToLowerInvariant()).Select(match => match.Value);

    public static IEnumerable<string> RemoveStopWords(IEnumerable<string> words) =>
        words.Where(word => !StopWords.Contains(word));

    public static IEnumerable<string> Terms(string text) => RemoveStopWords(Tokenize(text));
}

public class InvertedIndex
{
    private readonly Dictionary<string, HashSet<int>> _index = new();
    private readonly Dictionary<int, Document> _documents = new();

    public int DocumentCount => _documents.Count;
    public int UniqueWords => _index.Count;

    public void AddDocument(Document doc)
    {
        _documents.Add(doc.Id, doc);
        foreach (var word in TextProcessor.Terms(doc.Content))
        {
            if (!_index.TryGetValue(word, out var ids))
            {
                ids = new HashSet<int>();
                _index.Add(word, ids);
            }

            ids.Add(doc.Id);
        }
    }

    public List<Document> SearchSingleWord(string word) => SearchMultipleWords(word);

    public List<Document> SearchMultipleWords(params string[] words)
    {
        HashSet<int>? result = null;
        foreach (var word in words.SelectMany(TextProcessor.Terms))
        {
            if (!_index.TryGetValue(word, out var ids))
            {
                return new List<Document>();
            }

            if (result is null)
            {
                result = new HashSet<int>(ids);
            }
            else
            {
                result.IntersectWith(ids);
            }
        }

        return result is null ? new List<Document>() : GetDocuments(result);
    }

    public List<Document> SearchAnyWord(params string[] words)
    {
        var result = new HashSet<int>();
        foreach (var word in words.SelectMany(TextProcessor.Terms))
        {
            if (_index.TryGetValue(word, out var ids))
            {
                result.UnionWith(ids);
            }
        }

        return GetDocuments(result);
    }

    private List<Document> GetDocuments(HashSet<int> ids) =>
        ids.Order().Select(id => _documents[id]).ToList();
}

public static class PerformanceTest
{
    public static void CompareSearchMethods(List<Document> documents, string searchWord)
    {
        var timer = Stopwatch.StartNew();
        var index = new InvertedIndex();
        foreach (var document in documents)
        {
            index.AddDocument(document);
        }

        timer.Stop();
        double buildMs = timer.Elapsed.TotalMilliseconds;
        var normalizedWord = TextProcessor.Terms(searchWord).Single();
        List<Document> LinearSearch() => documents
            .Where(doc => TextProcessor.Terms(doc.Content).Contains(normalizedWord))
            .OrderBy(doc => doc.Id)
            .ToList();

        var linear = LinearSearch();
        var indexed = index.SearchSingleWord(searchWord);
        if (!linear.Select(doc => doc.Id).SequenceEqual(indexed.Select(doc => doc.Id)))
        {
            throw new InvalidOperationException("Результати пошуку відрізняються.");
        }

        const int repetitions = 20;
        timer.Restart();
        for (int i = 0; i < repetitions; i++)
        {
            LinearSearch();
        }

        timer.Stop();
        double linearMs = timer.Elapsed.TotalMilliseconds / repetitions;

        timer.Restart();
        for (int i = 0; i < repetitions; i++)
        {
            index.SearchSingleWord(searchWord);
        }

        timer.Stop();
        double indexMs = timer.Elapsed.TotalMilliseconds / repetitions;
        Console.WriteLine(FormattableString.Invariant(
            $"{documents.Count} | {linearMs:F4} | {indexMs:F4} | {buildMs:F4} | {linear.Count}"));
    }
}
