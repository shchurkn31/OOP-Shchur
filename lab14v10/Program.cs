using System.Diagnostics;
using System.Text;

namespace Lab14v10;

public record AppEvent(int Id, string Type, string Message)
{
    public override string ToString() => $"#{Id} [{Type}] {Message}";
}

public class EventProcessor
{
    private readonly HashSet<string> _eventTypes = new();
    private readonly Queue<AppEvent> _eventQueue = new();
    private readonly Stack<AppEvent> _history = new();
    private int _nextId = 1;

    public int QueueCount => _eventQueue.Count;
    public int HistoryCount => _history.Count;

    public bool HasType(string type) => _eventTypes.Contains(type);

    public IReadOnlyCollection<string> EventTypes => _eventTypes;

    public HashSet<string> TypesUnion(IEnumerable<string> other)
    {
        var copy = new HashSet<string>(_eventTypes);
        copy.UnionWith(other);
        return copy;
    }

    public HashSet<string> TypesIntersect(IEnumerable<string> other)
    {
        var copy = new HashSet<string>(_eventTypes);
        copy.IntersectWith(other);
        return copy;
    }

    public HashSet<string> TypesExcept(IEnumerable<string> other)
    {
        var copy = new HashSet<string>(_eventTypes);
        copy.ExceptWith(other);
        return copy;
    }

    public AppEvent Publish(string type, string message)
    {
        var ev = new AppEvent(_nextId++, type, message);
        _eventQueue.Enqueue(ev);
        bool isNewType = _eventTypes.Add(type);
        Console.WriteLine($"  Додано до черги: {ev} " +
                          (isNewType ? "(новий тип події)" : "(тип уже відомий)"));
        return ev;
    }

    public AppEvent? PeekNext() => _eventQueue.Count > 0 ? _eventQueue.Peek() : null;

    public AppEvent? ProcessNext()
    {
        if (_eventQueue.Count == 0)
        {
            return null;
        }

        var ev = _eventQueue.Dequeue();
        _history.Push(ev);
        Console.WriteLine($"  Оброблено: {ev}");
        return ev;
    }

    public void ProcessAll()
    {
        while (_eventQueue.Count > 0)
        {
            ProcessNext();
        }
    }

    public AppEvent? PeekLastProcessed() => _history.Count > 0 ? _history.Peek() : null;

    public AppEvent? UndoLast()
    {
        if (_history.Count == 0)
        {
            return null;
        }

        var ev = _history.Pop();
        Console.WriteLine($"  Скасовано (знято з історії): {ev}");
        return ev;
    }

    public void PrintHistory()
    {
        if (_history.Count == 0)
        {
            Console.WriteLine("  Історія порожня.");
            return;
        }

        foreach (var ev in _history)
        {
            Console.WriteLine($"  {ev}");
        }
    }
}

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var processor = new EventProcessor();

        Console.WriteLine("=== Лабораторна робота 14, варіант 10: Обробка подій ===\n");

        Console.WriteLine("1. Надходження подій (Queue.Enqueue + HashSet.Add):");
        processor.Publish("Click", "Натиснуто кнопку «Зберегти»");
        processor.Publish("KeyPress", "Натиснуто клавішу Enter");
        processor.Publish("Click", "Натиснуто кнопку «Скасувати»");
        processor.Publish("Error", "Файл не знайдено");
        processor.Publish("KeyPress", "Натиснуто клавішу Esc");
        processor.Publish("Timer", "Спрацював таймер автозбереження");
        Console.WriteLine($"  Подій у черзі: {processor.QueueCount}\n");

        Console.WriteLine("2. Унікальні типи подій (HashSet):");
        Console.WriteLine($"  Кількість унікальних типів: {processor.EventTypes.Count}");
        Console.WriteLine($"  Типи: {string.Join(", ", processor.EventTypes)}");
        Console.WriteLine($"  Contains(\"Error\")   -> {processor.HasType("Error")}");
        Console.WriteLine($"  Contains(\"Network\") -> {processor.HasType("Network")}\n");

        Console.WriteLine("3. Обробка перших 4 подій (Queue.Peek / Dequeue):");
        Console.WriteLine($"  Наступна подія (Peek): {processor.PeekNext()}");
        for (int i = 0; i < 4; i++)
        {
            processor.ProcessNext();
        }
        Console.WriteLine($"  Залишилось у черзі: {processor.QueueCount}\n");

        Console.WriteLine("4. Історія оброблених подій (Stack, від останньої до першої):");
        processor.PrintHistory();
        Console.WriteLine();

        Console.WriteLine("5. Скасування останньої обробленої події (Stack.Pop):");
        processor.UndoLast();
        Console.WriteLine($"  Тепер на вершині історії: {processor.PeekLastProcessed()}\n");

        Console.WriteLine("6. Обробка решти подій:");
        processor.ProcessAll();
        Console.WriteLine("  Історія після обробки:");
        processor.PrintHistory();
        Console.WriteLine();

        Console.WriteLine("7. Операції над множинами типів (HashSet):");
        var handlers = new[] { "Click", "Error", "Network", "Mouse" };
        Console.WriteLine($"  Типи, для яких є обробники: {string.Join(", ", handlers)}");
        Console.WriteLine($"  UnionWith     : {string.Join(", ", processor.TypesUnion(handlers))}");
        Console.WriteLine($"  IntersectWith : {string.Join(", ", processor.TypesIntersect(handlers))}");
        Console.WriteLine($"  ExceptWith    : {string.Join(", ", processor.TypesExcept(handlers))}\n");

        Console.WriteLine("8. Порівняння продуктивності пошуку (List vs HashSet):");
        ComparePerformance();

        Console.WriteLine("\nГотово.");
    }

    private static void ComparePerformance()
    {
        const int n = 20000;
        var list = new List<string>();
        var set = new HashSet<string>();
        for (int i = 0; i < n; i++)
        {
            var s = "Type" + i;
            list.Add(s);
            set.Add(s);
        }

        int found = 0;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            if (list.Contains("Type" + i))
            {
                found++;
            }
        }
        sw.Stop();
        Console.WriteLine($"  List<string>.Contains   : {sw.ElapsedMilliseconds} мс (знайдено {found})");

        found = 0;
        sw.Restart();
        for (int i = 0; i < n; i++)
        {
            if (set.Contains("Type" + i))
            {
                found++;
            }
        }
        sw.Stop();
        Console.WriteLine($"  HashSet<string>.Contains: {sw.ElapsedMilliseconds} мс (знайдено {found})");
    }
}