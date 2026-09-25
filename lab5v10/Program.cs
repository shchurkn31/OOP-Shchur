using System;
using System.Runtime.InteropServices;

namespace ResourceManagementDemo
{
    public class ImageBuffer : IDisposable
    {
        private byte[] _managedPixelData;
        private IntPtr _unmanagedBuffer;
        private bool _disposed = false;

        public string ImageName { get; }

        public ImageBuffer(string imageName, int bufferSize)
        {
            ImageName = imageName;
            _managedPixelData = new byte[bufferSize];
            _unmanagedBuffer = Marshal.AllocHGlobal(bufferSize);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _managedPixelData = null;
            }

            if (_unmanagedBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_unmanagedBuffer);
                _unmanagedBuffer = IntPtr.Zero;
            }

            _disposed = true;
        }

        ~ImageBuffer()
        {
            Dispose(false);
        }

        public void ProcessImage()
        {
            if (_disposed)
                throw new ObjectDisposedException(ImageName);

            Console.WriteLine($"Processing {ImageName}");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            using (var img1 = new ImageBuffer("Photo_1.png", 1024))
            {
                img1.ProcessImage();
            }

            var img2 = new ImageBuffer("Photo_2.png", 2048);
            img2.ProcessImage();
            img2.Dispose();
            img2.Dispose();

            CreateUnreferencedImage();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        static void CreateUnreferencedImage()
        {
            var img3 = new ImageBuffer("Photo_3.png", 4096);
            img3.ProcessImage();
        }
    }
}