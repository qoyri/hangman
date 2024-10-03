using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using HangmanApp.ViewModels;
using LottieSharp.WPF;

namespace HangmanApp.Views
{
    public partial class HomeView : UserControl
    {
        private string _tempJsonFile;

        public HomeView(HomeViewModel homeViewModel)
        {
            InitializeComponent();
            if (homeViewModel == null)
            {
                throw new ArgumentNullException(nameof(homeViewModel));
            }

            DataContext = homeViewModel;

            // Define path to settings.json and set it to the LottieAnimationView
            LoadJsonResource("settings.json");
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
                var lottieAnimationView = (LottieAnimationView)this.FindName("LottieAnimationViewControl");
                if (lottieAnimationView != null)
                {
                    lottieAnimationView.FileName = _tempJsonFile;

                    // Optional: Force refresh (depends on the Lottie implementation)
                    lottieAnimationView.InvalidateVisual();

                }

                var storyboard = (Storyboard)this.Resources["UserControlLoadAnimation"];
                storyboard.Begin();
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}