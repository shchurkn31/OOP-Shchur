using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab16;

public class Option
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;

    public override string ToString() => $"[Option #{Id}: {Value}]";
}

public class Feature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Option> Options { get; set; } = new();

    public override string ToString() => $"[Feature #{Id}: {Name} ({Options.Count} опцій)]";
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Feature> Features { get; set; } = new();

    public override string ToString() => $"[Product #{Id}: {Name} ({Features.Count} характеристик)]";
}

public static class ProductExtensions
{
    public static bool HasFeature(this Product product, string featureName)
    {
        if (product == null || string.IsNullOrWhiteSpace(featureName))
            return false;

        return product.Features.Any(f => 
            f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase));
    }
}

internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        List<Product> products = new()
        {
            new Product
            {
                Id = 1,
                Name = "Ноутбук ASUS ROG",
                Features = new List<Feature>
                {
                    new Feature { Id = 101, Name = "Дисплей", Options = new List<Option> { new Option { Id = 1, Value = "15.6 IPS" }, new Option { Id = 2, Value = "144 Hz" } } },
                    new Feature { Id = 102, Name = "Процесор", Options = new List<Option> { new Option { Id = 3, Value = "Intel Core i7-13700H" }, new Option { Id = 4, Value = "14 ядер" } } }
                }
            },
            new Product
            {
                Id = 2,
                Name = "Смартфон Samsung Galaxy S24",
                Features = new List<Feature>
                {
                    new Feature { Id = 103, Name = "Дисплей", Options = new List<Option> { new Option { Id = 5, Value = "6.7 Dynamic AMOLED" }, new Option { Id = 6, Value = "120 Hz" } } },
                    new Feature { Id = 104, Name = "Камера", Options = new List<Option> { new Option { Id = 7, Value = "200 MP" }, new Option { Id = 8, Value = "8K Video" } } }
                }
            },
            new Product
            {
                Id = 3,
                Name = "Планшет Apple iPad Pro",
                Features = new List<Feature>
                {
                    new Feature { Id = 105, Name = "Дисплей", Options = new List<Option> { new Option { Id = 9, Value = "11 Liquid Retina" } } },
                    new Feature { Id = 106, Name = "Пам'ять", Options = new List<Option> { new Option { Id = 10, Value = "256 GB" }, new Option { Id = 11, Value = "8 GB RAM" } } }
                }
            },
            new Product
            {
                Id = 4,
                Name = "Монітор Dell UltraSharp",
                Features = new List<Feature>
                {
                    new Feature { Id = 107, Name = "Дисплей", Options = new List<Option> { new Option { Id = 12, Value = "27 дюймів 4K" }, new Option { Id = 13, Value = "60 Hz" } } },
                    new Feature { Id = 108, Name = "Порти", Options = new List<Option> { new Option { Id = 14, Value = "HDMI 2.1" }, new Option { Id = 15, Value = "USB-C Hub" } } }
                }
            },
            new Product
            {
                Id = 5,
                Name = "Навушники Sony WH-1000XM5",
                Features = new List<Feature>
                {
                    new Feature { Id = 109, Name = "Акумулятор", Options = new List<Option> { new Option { Id = 16, Value = "30 годин" }, new Option { Id = 17, Value = "Fast Charge" } } },
                    new Feature { Id = 110, Name = "Звук", Options = new List<Option> { new Option { Id = 18, Value = "Active Noise Canceling" }, new Option { Id = 19, Value = "Hi-Res Audio" } } }
                }
            },
            new Product
            {
                Id = 6,
                Name = "Смарт-годинник Apple Watch Ultra",
                Features = new List<Feature>
                {
                    new Feature { Id = 111, Name = "Акумулятор", Options = new List<Option> { new Option { Id = 20, Value = "36 годин" } } },
                    new Feature { Id = 112, Name = "Захист", Options = new List<Option> { new Option { Id = 21, Value = "WR100" }, new Option { Id = 22, Value = "IP6X" } } }
                }
            },
            new Product
            {
                Id = 7,
                Name = "Ігрова консоль PlayStation 5",
                Features = new List<Feature>
                {
                    new Feature { Id = 113, Name = "Пам'ять", Options = new List<Option> { new Option { Id = 23, Value = "825 GB Custom SSD" } } },
                    new Feature { Id = 114, Name = "Процесор", Options = new List<Option> { new Option { Id = 24, Value = "AMD Zen 2" } } }
                }
            }
        };

        Console.WriteLine("=== 1. SelectMany ===");
        var allOptions = products
            .SelectMany(p => p.Features)
            .SelectMany(f => f.Options)
            .ToList();

        Console.WriteLine($"Усього опцій: {allOptions.Count}\n");
        foreach (var option in allOptions)
        {
            Console.WriteLine($" - {option}");
        }

        Console.WriteLine("\n=== 2. Пагінація ===");
        int pageSize = 3;
        int totalPages = (int)Math.Ceiling((double)products.Count / pageSize);

        for (int page = 1; page <= totalPages; page++)
        {
            Console.WriteLine($"\nСторінка {page} з {totalPages}:");
            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            foreach (var product in pagedProducts)
            {
                Console.WriteLine($"  {product.Id}. {product.Name}");
            }
        }

        Console.WriteLine("\n=== 3. Custom Extension ===");
        string targetFeature = "Камера";
        Console.WriteLine($"Продукти з характеристикою '{targetFeature}':");

        var productsWithCamera = products.Where(p => p.HasFeature(targetFeature));
        foreach (var product in productsWithCamera)
        {
            Console.WriteLine($" - {product.Name}");
        }
    }
}