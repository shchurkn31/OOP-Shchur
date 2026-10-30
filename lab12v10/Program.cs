using System;
using System.Collections.Generic;

public static class FunctionalUtils
{
    public static List<TResult> Map<T, TResult>(List<T> list, Func<T, TResult> mapper)
    {
        var result = new List<TResult>();
        foreach (var item in list)
        {
            result.Add(mapper(item));
        }
        return result;
    }

    public static List<T> Filter<T>(List<T> list, Func<T, bool> predicate)
    {
        var result = new List<T>();
        foreach (var item in list)
        {
            if (predicate(item))
                result.Add(item);
        }
        return result;
    }

    public static T? FirstOrDefault<T>(List<T> list, Predicate<T> predicate)
    {
        foreach (var item in list)
        {
            if (predicate(item))
                return item;
        }
        return default;
    }

    public static T? LastOrDefault<T>(List<T> list, Predicate<T> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate(list[i]))
                return list[i];
        }
        return default;
    }

    public static T Single<T>(List<T> list, Predicate<T> predicate)
    {
        T? found = default;
        int count = 0;
        foreach (var item in list)
        {
            if (predicate(item))
            {
                found = item;
                count++;
            }
        }
        if (count == 0)
            throw new InvalidOperationException("Жодного елемента не знайдено");
        if (count > 1)
            throw new InvalidOperationException("Знайдено більше одного елемента");
        return found!;
    }

    public static void ForEach<T>(List<T> list, Action<T> action)
    {
        foreach (var item in list)
        {
            action(item);
        }
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string City { get; set; }

    public Person(string name, int age, string city)
    {
        Name = name;
        Age = age;
        City = city;
    }

    public override string ToString()
    {
        return $"{Name}, {Age} р., {City}";
    }
}

public class Program
{
    public static void Main()
    {
        var people = new List<Person>
        {
            new Person("Олена", 19, "Рівне"),
            new Person("Максим", 22, "Львів"),
            new Person("Ірина", 17, "Рівне"),
            new Person("Андрій", 25, "Київ"),
            new Person("Софія", 20, "Рівне")
        };
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var words = new List<string> { "яблуко", "груша", "слива", "абрикос", "вишня" };

        Console.WriteLine("=== Map ===");
        var squares = FunctionalUtils.Map(numbers, x => x * x);
        Console.WriteLine("Квадрати: " + string.Join(", ", squares));
        var lengths = FunctionalUtils.Map(words, w => w.Length);
        Console.WriteLine("Довжини слів: " + string.Join(", ", lengths));
        var names = FunctionalUtils.Map(people, p => p.Name);
        Console.WriteLine("Імена: " + string.Join(", ", names));

        Console.WriteLine("\n=== Filter ===");
        var even = FunctionalUtils.Filter(numbers, x => x % 2 == 0);
        Console.WriteLine("Парні: " + string.Join(", ", even));
        var withA = FunctionalUtils.Filter(words, w => w.StartsWith("а"));
        Console.WriteLine("Слова на 'а': " + string.Join(", ", withA));
        var adults = FunctionalUtils.Filter(people, p => p.Age >= 18);
        Console.WriteLine("Повнолітні:");
        FunctionalUtils.ForEach(adults, p => Console.WriteLine("  " + p));

        Console.WriteLine("\n=== FirstOrDefault ===");
        Console.WriteLine("Перше число > 5: " + FunctionalUtils.FirstOrDefault(numbers, x => x > 5));
        Console.WriteLine("Перше число > 100: " + FunctionalUtils.FirstOrDefault(numbers, x => x > 100));
        Console.WriteLine("Перше слово довше за 5: " + FunctionalUtils.FirstOrDefault(words, w => w.Length > 5));
        var firstRivne = FunctionalUtils.FirstOrDefault(people, p => p.City == "Рівне");
        Console.WriteLine("Перший з Рівного: " + firstRivne);
        var nobody = FunctionalUtils.FirstOrDefault(people, p => p.City == "Одеса");
        Console.WriteLine("Хтось з Одеси: " + (nobody == null ? "null" : nobody.ToString()));

        Console.WriteLine("\n=== LastOrDefault ===");
        Console.WriteLine("Останнє парне: " + FunctionalUtils.LastOrDefault(numbers, x => x % 2 == 0));
        Console.WriteLine("Останнє слово на 'с': " + FunctionalUtils.LastOrDefault(words, w => w.StartsWith("с")));
        var lastRivne = FunctionalUtils.LastOrDefault(people, p => p.City == "Рівне");
        Console.WriteLine("Останній з Рівного: " + lastRivne);

        Console.WriteLine("\n=== Single ===");
        Console.WriteLine("Єдине число 7: " + FunctionalUtils.Single(numbers, x => x == 7));
        Console.WriteLine("Єдине слово 'груша': " + FunctionalUtils.Single(words, w => w == "груша"));
        Console.WriteLine("Єдиний з Києва: " + FunctionalUtils.Single(people, p => p.City == "Київ"));

        try
        {
            FunctionalUtils.Single(people, p => p.City == "Рівне");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Помилка (кілька збігів): " + ex.Message);
        }

        try
        {
            FunctionalUtils.Single(numbers, x => x > 100);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Помилка (немає збігів): " + ex.Message);
        }
    }
}