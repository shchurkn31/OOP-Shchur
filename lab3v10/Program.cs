using System;

namespace OOP
{
    public class ImageBuffer : IDisposable
    {
        private bool _disposed = false;
        private bool _isAllocated;
        private int _width;
        private int _height;

        public int Width => _width;
        public int Height => _height;
        public bool IsAllocated => _isAllocated;

        public ImageBuffer(int width, int height)
        {
            _width = width;
            _height = height;
            _isAllocated = true;
            Console.WriteLine($"Створено буфер зображення {_width}x{_height}");
        }

        public void DrawPixel(int x, int y)
        {
            if (_disposed || !_isAllocated)
            {
                Console.WriteLine($"Помилка: буфер вже закритий, неможливо намалювати піксель ({x}, {y})");
                return;
            }
            Console.WriteLine($"Малюємо піксель у точці ({x}, {y})");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.WriteLine("Звільнення керованих ресурсів");
                }

                if (_isAllocated)
                {
                    Console.WriteLine("Звільнення некерованого буфера пам'яті");
                    _isAllocated = false;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ImageBuffer()
        {
            Console.WriteLine("Спрацював деструктор ImageBuffer");
            Dispose(false);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("1. Перевірка using:");
            using (ImageBuffer buffer1 = new ImageBuffer(1920, 1080))
            {
                buffer1.DrawPixel(100, 200);
            }

            Console.WriteLine("\n2. Явний виклик Dispose():");
            ImageBuffer buffer2 = new ImageBuffer(800, 600);
            buffer2.DrawPixel(50, 50);
            buffer2.Dispose();

            Console.WriteLine("\n3. Очищення через GC:");
            CreateBuffer();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        static void CreateBuffer()
        {
            ImageBuffer buffer3 = new ImageBuffer(1024, 768);
            buffer3.DrawPixel(10, 20);
        }
    }
}