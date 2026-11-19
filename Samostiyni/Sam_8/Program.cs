using IndependentWork7;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var cache = new Cache<int, string>(2);
cache.Add(1, "Клавіатура");
cache.Add(2, "Миша");
Console.WriteLine($"Ключ 1: {cache.Get(1)}");
cache.Add(3, "Монітор");
Console.WriteLine($"Ключ 1 залишився: {cache.ContainsKey(1)}");
Console.WriteLine($"Кількість: {cache.Count}");
