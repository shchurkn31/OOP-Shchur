using System;

class Computer
{
private string cpu;
private int ram;

public int Storage { get; set; }

public Computer(string cpu, int ram, int storage)
{
    this.cpu = cpu;
    this.ram = ram;
    Storage = storage;
}

public void RunBenchmark()
{
    Console.WriteLine($"Computer: CPU — {cpu}, RAM — {ram} GB, Storage — {Storage} GB");
    Console.WriteLine($"Benchmark result: {ram * 100 + Storage / 10} points");
    Console.WriteLine();
}

Computer()
{
    Console.WriteLine($"Computer with CPU {cpu} destroyed.");
}


}

class Program
{
static void Main()
{
Computer computer1 = new Computer("Intel Core i5", 16, 512);
Computer computer2 = new Computer("AMD Ryzen 5", 16, 1000);
Computer computer3 = new Computer("Intel Core i7", 32, 1000);

    computer1.RunBenchmark();
    computer2.RunBenchmark();
    computer3.RunBenchmark();
}


}