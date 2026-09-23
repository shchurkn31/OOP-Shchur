using System;

namespace CustomTypes
{
    public class Percentage
    {
        private double _value;

        public Percentage(double value)
        {
            Value = value;
        }

        public double Value
        {
            get => _value;
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Значення має бути від 0 до 100.");
                }
                _value = value;
            }
        }

        public static Percentage MaxPercentage => new Percentage(100);

        public double this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _value,
                    1 => 100.0 - _value,
                    _ => throw new IndexOutOfRangeException("Допустимі індекси 0 або 1.")
                };
            }
        }

        public static Percentage operator +(Percentage a, Percentage b)
        {
            if (a is null || b is null) throw new ArgumentNullException();
            return new Percentage(a.Value + b.Value);
        }

        public static Percentage operator *(Percentage p, double factor)
        {
            if (p is null) throw new ArgumentNullException(nameof(p));
            return new Percentage(p.Value * factor);
        }

        public static Percentage operator *(double factor, Percentage p)
        {
            return p * factor;
        }

        public static bool operator ==(Percentage left, Percentage right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Percentage left, Percentage right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Percentage other)
            {
                return Math.Abs(_value - other._value) < 1e-9;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{_value}%";
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Percentage p1 = new Percentage(25.5);
            Percentage p2 = new Percentage(40.0);
            Percentage p3 = new Percentage(25.5);

            Console.WriteLine("Створені обєкти:");
            Console.WriteLine($"p1: {p1}");
            Console.WriteLine($"p2: {p2}");
            Console.WriteLine($"p3: {p3}");

            Console.WriteLine("Перевірка валідації:");
            try
            {
                Percentage pInvalid = new Percentage(150);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Статичне значення:");
            Console.WriteLine(Percentage.MaxPercentage);

            Console.WriteLine("Індексатор:");
            Console.WriteLine($"Значення: {p1[0]}");
            Console.WriteLine($"Залишок: {p1[1]}");

            Console.WriteLine("Оператори:");
            Console.WriteLine($"Додавання: {p1 + p2}");
            Console.WriteLine($"Множення: {p1 * 2}");

            Console.WriteLine("Порівняння:");
            Console.WriteLine($"p1 дорівнює p3: {p1 == p3}");
            Console.WriteLine($"p1 дорівнює p2: {p1 == p2}");
            Console.WriteLine($"p1 не дорівнює p2: {p1 != p2}");
            Console.WriteLine($"Equals: {p1.Equals(p3)}");
            Console.WriteLine($"GetHashCode p1: {p1.GetHashCode()}");
            Console.WriteLine($"GetHashCode p3: {p3.GetHashCode()}");
        }
    }
}