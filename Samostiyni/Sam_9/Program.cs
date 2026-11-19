using System.Text.RegularExpressions;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var files = new FileMetadataCache();
files.Add(new FileMetadata("notes.txt", 128, new DateTime(2026, 9, 30), "demo-hash"));
Console.WriteLine($"Розмір notes.txt: {files.Get("NOTES.TXT")?.Size} байт");
Console.WriteLine($"Видалено: {files.Remove("notes.txt")}, залишився: {files.Contains("notes.txt")}");

var tags = new TagManager();
tags.AddTag("article-1", "C#");
tags.AddTag("article-1", "c#");
tags.AddTag("article-1", ".NET");
tags.AddTag("article-2", "C#");
tags.AddTag("article-2", "LINQ");
tags.RemoveTag("article-2", "LINQ");
Console.WriteLine($"Унікальні теги: {string.Join(", ", tags.GetAllUniqueTags().Order())}");
Console.WriteLine($"Спільні теги: {string.Join(", ", tags.FindCommonTags("article-1", "article-2"))}");

var index = new SearchIndex();
index.AddDocument(1, "C# і колекції");
index.AddDocument(2, "C# і LINQ");
Console.WriteLine($"C#: {string.Join(", ", index.Search("C#").Order())}");
Console.WriteLine($"C# AND LINQ: {string.Join(", ", index.SearchMultiple("C#", "LINQ").Order())}");

public record FileMetadata(string FileName, long Size, DateTime LastModified, string Hash);

public class FileMetadataCache
{
    private readonly Dictionary<string, FileMetadata> _cache = new(StringComparer.OrdinalIgnoreCase);

    public void Add(FileMetadata metadata) => _cache[metadata.FileName] = metadata;
    public FileMetadata? Get(string fileName) => _cache.GetValueOrDefault(fileName);
    public bool Remove(string fileName) => _cache.Remove(fileName);
    public bool Contains(string fileName) => _cache.ContainsKey(fileName);
}

public class TagManager
{
    private readonly Dictionary<string, HashSet<string>> _articleTags = new();

    public void AddTag(string articleId, string tag)
    {
        if (!_articleTags.TryGetValue(articleId, out var tags))
        {
            tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _articleTags.Add(articleId, tags);
        }

        tags.Add(tag);
    }

    public void RemoveTag(string articleId, string tag)
    {
        if (_articleTags.TryGetValue(articleId, out var tags))
        {
            tags.ExceptWith(new[] { tag });
        }
    }

    public HashSet<string> GetAllUniqueTags()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tags in _articleTags.Values)
        {
            result.UnionWith(tags);
        }

        return result;
    }

    public HashSet<string> FindCommonTags(string articleId1, string articleId2)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (_articleTags.TryGetValue(articleId1, out var first) &&
            _articleTags.TryGetValue(articleId2, out var second))
        {
            result.UnionWith(first);
            result.IntersectWith(second);
        }

        return result;
    }
}

public class SearchIndex
{
    private readonly Dictionary<string, HashSet<int>> _invertedIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<int> _documentIds = new();
    private static readonly Regex WordPattern = new(@"[\p{L}\p{N}]+[#]?", RegexOptions.Compiled);

    public void AddDocument(int docId, string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (!_documentIds.Add(docId))
        {
            throw new ArgumentException("Документ із таким ID вже додано.", nameof(docId));
        }

        foreach (Match match in WordPattern.Matches(content))
        {
            if (!_invertedIndex.TryGetValue(match.Value, out var documents))
            {
                documents = new HashSet<int>();
                _invertedIndex.Add(match.Value, documents);
            }

            documents.Add(docId);
        }
    }

    public HashSet<int> Search(string word)
    {
        var match = WordPattern.Match(word);
        return match.Success && _invertedIndex.TryGetValue(match.Value, out var documents)
            ? new HashSet<int>(documents)
            : new HashSet<int>();
    }

    public HashSet<int> SearchMultiple(params string[] words)
    {
        if (words.Length == 0)
        {
            return new HashSet<int>();
        }

        var result = Search(words[0]);
        foreach (var word in words.Skip(1))
        {
            result.IntersectWith(Search(word));
        }

        return result;
    }
}
