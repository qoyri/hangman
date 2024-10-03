using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Threading.Tasks;

namespace HangmanApp.Services
{
    public class MusicService
    {
        private static readonly Lazy<MusicService> _instance = new Lazy<MusicService>(() => new MusicService());
        public static MusicService Instance => _instance.Value;

        private readonly MediaPlayer _mediaPlayer;

        private MusicService()
        {
            _mediaPlayer = new MediaPlayer();
            // Charger le volume de la musique initial
            _mediaPlayer.Volume = SecureStorage.GetVolume();
        }

        public void PlayMusic(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourcePath = $"hangman.Resources.{resourceName}";

                using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (resourceStream == null)
                    {
                        throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly manifest.");
                    }

                    var tempFile = Path.GetTempFileName() + Path.GetExtension(resourceName);
                    using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }

                    _mediaPlayer.Open(new Uri(tempFile));
                    _mediaPlayer.MediaEnded += (s, e) => _mediaPlayer.Position = TimeSpan.Zero; // Repeat the music

                    _mediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing music: {ex.Message}");
            }
        }

        public void StopMusic()
        {
            _mediaPlayer.Stop();
        }

        public void SetVolume(double volume)
        {
            _mediaPlayer.Volume = volume;
        }

        public void PrintAllResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                Console.WriteLine(resourceName);
            }
        }
    }
}