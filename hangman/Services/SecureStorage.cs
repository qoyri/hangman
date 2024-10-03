using System;
using System.IO;

namespace HangmanApp.Services
{
    public static class SecureStorage
    {
        private static readonly string TokenFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HangmanApp",
                "token.txt");

        private static readonly string VolumeFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HangmanApp",
                "volume.txt");

        public static void SaveToken(string token)
        {
            var directory = Path.GetDirectoryName(TokenFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(TokenFilePath, token);
        }

        public static string GetToken()
        {
            if (File.Exists(TokenFilePath))
            {
                return File.ReadAllText(TokenFilePath);
            }

            return null;
        }

        public static void ClearToken()
        {
            if (File.Exists(TokenFilePath))
            {
                File.Delete(TokenFilePath);
            }
        }


        public static void SaveVolume(double volume)
        {
            var directory = Path.GetDirectoryName(VolumeFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(VolumeFilePath, volume.ToString());
        }

        public static double GetVolume()
        {
            if (File.Exists(VolumeFilePath))
            {
                string volumeString = File.ReadAllText(VolumeFilePath);
                if (double.TryParse(volumeString, out double volume))
                {
                    return volume;
                }
            }

            return 0.5; // Default volume
        }

        public static void ClearVolume()
        {
            if (File.Exists(VolumeFilePath))
            {
                File.Delete(VolumeFilePath);
            }
        }
    }
}