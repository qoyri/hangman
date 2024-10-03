using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace HangmanApp.Services
{
    public class ImageService
    {
        private static readonly Lazy<ImageService> _instance = new Lazy<ImageService>(() => new ImageService());
        public static ImageService Instance => _instance.Value;

        private ImageService()
        {
            Console.WriteLine("ImageService initialized.");
        }

        public BitmapImage LoadImage(string resourceName)
        {
            Console.WriteLine($"Attempting to load image: {resourceName}");

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourcePath = $"HangmanApp.Resources.{resourceName}";
                Console.WriteLine($"Looking for resource: {resourcePath}");

                using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (resourceStream == null)
                    {
                        Console.WriteLine($"Resource '{resourceName}' not found in assembly manifest.");
                        throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly manifest.");
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        resourceStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = memoryStream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.DecodePixelWidth = 60; // Use decode size for performance, change as needed
                        bitmap.DecodePixelHeight = 60; // Use decode size for performance, change as needed
                        bitmap.EndInit();
                        bitmap.Freeze(); // Freeze to make it cross-thread accessible

                        Console.WriteLine("Image loaded successfully.");
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                return null; // Or return a default image
            }
        }
    }
}