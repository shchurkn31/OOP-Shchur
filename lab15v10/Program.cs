using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Lab15v10;

public class Location
{
    public int Id { get; }
    public string Name { get; }
    public string Warehouse { get; }

    public Location(int id, string name, string warehouse)
    {
        Id = id;
        Name = name;
        Warehouse = warehouse;
    }
}

public class Item
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; }
    public int LocationId { get; }

    public Item(int id, string name, int quantity, int locationId)
    {
        Id = id;
        Name = name;
        Quantity = quantity;
        LocationId = locationId;
    }
}

public static class InventoryData
{
    public static List<Location> CreateLocations() => new()
    {
        new(1, "Shelf A1", "Main Warehouse"),
        new(2, "Shelf A2", "Main Warehouse"),
        new(3, "Shelf B1", "Main Warehouse"),
        new(4, "Cold Room", "Main Warehouse"),
        new(5, "Rack N1", "North Warehouse"),
        new(6, "Rack N2", "North Warehouse"),
        new(7, "Rack N3", "North Warehouse"),
        new(8, "Zone S1", "South Warehouse"),
        new(9, "Zone S2", "South Warehouse"),
        new(10, "Zone S3", "South Warehouse")
    };

    public static List<Item> CreateItems() => new()
    {
        new(1, "Hammer", 5, 1),
        new(2, "Screwdriver", 24, 1),
        new(3, "Wrench", 8, 2),
        new(4, "Pliers", 40, 2),
        new(5, "Drill", 3, 3),
        new(6, "Saw", 12, 3),
        new(7, "Tape Measure", 7, 4),
        new(8, "Safety Gloves", 150, 4),
        new(9, "Helmet", 9, 5),
        new(10, "Nails Box", 300, 5),
        new(11, "Screws Box", 220, 6),
        new(12, "Paint Bucket", 6, 6),
        new(13, "Brush", 35, 7),
        new(14, "Ladder", 2, 7),
        new(15, "Flashlight", 18, 8),
        new(16, "Batteries Pack", 95, 8),
        new(17, "Extension Cord", 4, 9),
        new(18, "Duct Tape", 60, 9),
        new(19, "Level", 11, 10),
        new(20, "Glue", 14, 10)
    };
}

public class Program
{
    private const string MainWarehouse = "Main Warehouse";

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var locations = InventoryData.CreateLocations();
        var items = InventoryData.CreateItems();

        Console.WriteLine("=== Лабораторна робота 15, варіант 10: Управління інвентарем ===\n");
        Console.WriteLine($"Локацій: {locations.Count}, предметів: {items.Count}\n");

        Query1(items, locations);
        Query2(items, locations);
        Query3(items, locations);
        Query4(items, locations);
        DeferredVsImmediate(items);
        BenchmarkLarge(locations);

        Console.WriteLine("\nГотово.");
    }

    private static (T Result, double Ms) Measure<T>(Func<T> query)
    {
        var sw = Stopwatch.StartNew();
        var result = query();
        sw.Stop();
        return (result, sw.Elapsed.TotalMilliseconds);
    }

    private static string Ms(double value) => value.ToString("F4", CultureInfo.InvariantCulture) + " мс";

    private static void PrintTimes(double queryMs, double methodMs)
    {
        Console.WriteLine($"  Час (синтаксис запитів): {Ms(queryMs)}");
        Console.WriteLine($"  Час (синтаксис методів): {Ms(methodMs)}");
    }

    private static void Query1(List<Item> items, List<Location> locations)
    {
        Console.WriteLine("1. Предмети з кількістю < 10 на складі \"Main Warehouse\" (Join + Where):");

        var (byQuery, queryMs) = Measure(() =>
            (from i in items
             join l in locations on i.LocationId equals l.Id
             where i.Quantity < 10 && l.Warehouse == MainWarehouse
             orderby i.Id
             select new { i.Name, i.Quantity, Location = l.Name }).ToList());

        var (byMethod, methodMs) = Measure(() =>
            items.Join(locations, i => i.LocationId, l => l.Id, (i, l) => new { Item = i, Location = l })
                 .Where(x => x.Item.Quantity < 10 && x.Location.Warehouse == MainWarehouse)
                 .OrderBy(x => x.Item.Id)
                 .Select(x => new { x.Item.Name, x.Item.Quantity, Location = x.Location.Name })
                 .ToList());

        foreach (var r in byQuery)
        {
            Console.WriteLine($"  {r.Name}: {r.Quantity} шт. ({r.Location})");
        }
        Console.WriteLine($"  Знайдено: {byQuery.Count} (синтаксис методів: {byMethod.Count})");
        PrintTimes(queryMs, methodMs);
        Console.WriteLine();
    }

    private static void Query2(List<Item> items, List<Location> locations)
    {
        Console.WriteLine("2. Предмети разом з назвами локацій (Join):");

        var (byQuery, queryMs) = Measure(() =>
            (from i in items
             join l in locations on i.LocationId equals l.Id
             select new { ItemName = i.Name, LocationName = l.Name }).ToList());

        var (byMethod, methodMs) = Measure(() =>
            items.Join(locations, i => i.LocationId, l => l.Id,
                       (i, l) => new { ItemName = i.Name, LocationName = l.Name }).ToList());

        foreach (var r in byQuery)
        {
            Console.WriteLine($"  {r.ItemName} -> {r.LocationName}");
        }
        Console.WriteLine($"  Рядків: {byQuery.Count} (синтаксис методів: {byMethod.Count})");
        PrintTimes(queryMs, methodMs);
        Console.WriteLine();
    }

    private static void Query3(List<Item> items, List<Location> locations)
    {
        Console.WriteLine("3. Загальна кількість предметів у кожній локації (GroupJoin + Sum):");

        var (byQuery, queryMs) = Measure(() =>
            (from l in locations
             join i in items on l.Id equals i.LocationId into group_
             select new { Location = l.Name, l.Warehouse, Total = group_.Sum(x => x.Quantity) }).ToList());

        var (byMethod, methodMs) = Measure(() =>
            locations.GroupJoin(items, l => l.Id, i => i.LocationId,
                                (l, group_) => new { Location = l.Name, l.Warehouse, Total = group_.Sum(x => x.Quantity) })
                     .ToList());

        foreach (var r in byQuery)
        {
            Console.WriteLine($"  {r.Location} ({r.Warehouse}): {r.Total}");
        }
        Console.WriteLine($"  Усього на всіх локаціях: {byMethod.Sum(r => r.Total)}");
        PrintTimes(queryMs, methodMs);
        Console.WriteLine();
    }

    private static void Query4(List<Item> items, List<Location> locations)
    {
        Console.WriteLine("4. Предмет з максимальною кількістю (Max / MaxBy):");

        var (byQuery, queryMs) = Measure(() =>
        {
            var max = items.Max(i => i.Quantity);
            return (from i in items
                    join l in locations on i.LocationId equals l.Id
                    where i.Quantity == max
                    select new { i.Name, i.Quantity, Location = l.Name, l.Warehouse }).First();
        });

        var (byMethod, methodMs) = Measure(() =>
        {
            var best = items.MaxBy(i => i.Quantity)!;
            var location = locations.First(l => l.Id == best.LocationId);
            return new { best.Name, best.Quantity, Location = location.Name, location.Warehouse };
        });

        Console.WriteLine($"  {byQuery.Name} — {byQuery.Quantity} шт. ({byQuery.Location}, {byQuery.Warehouse})");
        Console.WriteLine($"  Синтаксис методів дав той самий предмет: {byQuery.Name == byMethod.Name}");
        PrintTimes(queryMs, methodMs);
        Console.WriteLine();
    }

    private static void DeferredVsImmediate(List<Item> items)
    {
        Console.WriteLine("5. Відкладене та негайне виконання:");
        var local = new List<Item>(items);

        var sw = Stopwatch.StartNew();
        var deferred = local.Where(i => i.Quantity < 10);
        sw.Stop();
        Console.WriteLine($"  Створення запиту Where (відкладене): {Ms(sw.Elapsed.TotalMilliseconds)}");

        sw.Restart();
        var immediate = deferred.ToList();
        sw.Stop();
        Console.WriteLine($"  Виконання ToList() (негайне): {Ms(sw.Elapsed.TotalMilliseconds)}");
        Console.WriteLine($"  Елементів у ToList(): {immediate.Count}");

        local.Add(new Item(21, "Marker", 1, 1));
        Console.WriteLine($"  Після додавання предмета запит дає: {deferred.Count()}, а ToList() лишився: {immediate.Count}");
        Console.WriteLine();
    }

    private static void BenchmarkLarge(List<Location> locations)
    {
        const int count = 500_000;
        var random = new Random(42);
        var bigItems = new List<Item>(count);
        for (int i = 1; i <= count; i++)
        {
            bigItems.Add(new Item(i, "Item" + i, random.Next(0, 500), random.Next(1, locations.Count + 1)));
        }

        Console.WriteLine($"6. Продуктивність на великій колекції ({count} предметів, синтаксис методів):");

        var tests = new (string Name, Func<int> Run)[]
        {
            ("Запит 1 (Join + Where)", () => bigItems
                .Join(locations, i => i.LocationId, l => l.Id, (i, l) => new { Item = i, Location = l })
                .Where(x => x.Item.Quantity < 10 && x.Location.Warehouse == MainWarehouse)
                .ToList().Count),
            ("Запит 2 (Join)", () => bigItems
                .Join(locations, i => i.LocationId, l => l.Id, (i, l) => new { ItemName = i.Name, LocationName = l.Name })
                .ToList().Count),
            ("Запит 3 (GroupJoin + Sum)", () => locations
                .GroupJoin(bigItems, l => l.Id, i => i.LocationId, (l, g) => new { l.Name, Total = g.Sum(x => x.Quantity) })
                .ToList().Count),
            ("Запит 4 (MaxBy)", () => bigItems.MaxBy(i => i.Quantity)!.Id)
        };

        foreach (var (name, run) in tests)
        {
            run();
            var (_, ms) = Measure(run);
            Console.WriteLine($"  {name}: {Ms(ms)}");
        }
    }
}