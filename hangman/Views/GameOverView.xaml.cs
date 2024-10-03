using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using LottieSharp.WPF;

namespace HangmanApp.Views
{
    public partial class GameOverView : UserControl
    {
        private string _tempJsonFile;

        public GameOverView()
        {
            InitializeComponent();
            LoadJsonResource("game_over.json");
        }

        private void LoadJsonResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourcePath = $"hangman.Resources.images.{resourceName}";

                using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (resourceStream == null)
                    {
                        throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly manifest.");
                    }

                    _tempJsonFile = Path.Combine(Path.GetTempPath(), resourceName);
                    using (var fileStream = new FileStream(_tempJsonFile, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading JSON resource: {ex.Message}");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var lottieAnimationView = (LottieAnimationView)this.FindName("LottieGameOverAnimationView");
                if (lottieAnimationView != null && !string.IsNullOrEmpty(_tempJsonFile))
                {
                    lottieAnimationView.FileName = _tempJsonFile;

                    // Optional: Force refresh (depends on the Lottie implementation)
                    lottieAnimationView.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during UserControl_Loaded: {ex.Message}");
            }
        }
    }
}