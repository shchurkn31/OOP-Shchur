using System;

namespace OOP
{
    public class Computer
    {
        private string _cpu;
        private int _ramGB;
        private int _storageGB;

        public string CPU
        {
            get => _cpu;
            set => _cpu = string.IsNullOrWhiteSpace(value) ? "Intel i5" : value;
        }

        public int RAMGB
        {
            get => _ramGB;
            set
            {
                if (value <= 0)
                {
                    Console.WriteLine($"[Помилка] Обсяг RAM ({value} GB) має бути більше 0!");
                    _ramGB = 8;
                }
                else
                {
                    _ramGB = value;
                }
            }
        }

        public int StorageGB
        {
            get => _storageGB;
            set
            {
                if (value <= 0)
                {
                    Console.WriteLine($"[Помилка] Обсяг накопичувача ({value} GB) має бути більше 0!");
                    _storageGB = 256;
                }
                else
                {
                    _storageGB = value;
                }
            }
        }

        public Computer(string cpu, int ramGB, int storageGB)
        {
            CPU = cpu;
            RAMGB = ramGB;
            StorageGB = storageGB;
        }

        public Computer() : this("Intel i5", 8, 256)
        {
        }

        public void RunBenchmark()
        {
            Console.WriteLine($"Запуск бенчмарку для ПК [{CPU} | {RAMGB}GB RAM | {StorageGB}GB SSD]... Тест успішно пройдено!");
        }

        ~Computer()
        {
            Console.WriteLine($"[Destructor] Збирач сміття видалив ПК з процесором: \"{_cpu}\"");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Створення об'єктів ---");
            Computer pc1 = new Computer();
            Computer pc2 = new Computer("AMD Ryzen 7", 32, 1000);
            Computer pc3 = new Computer("Intel i9", -16, 0);

            Console.WriteLine("\n--- Виклик методів ---");
            pc1.RunBenchmark();
            pc2.RunBenchmark();
            pc3.RunBenchmark();

            Console.WriteLine("\n--- Очищення пам'яті (GC) ---");
            pc1 = null!;
            pc2 = null!;
            pc3 = null!;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}