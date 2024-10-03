using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HangmanApp.Services;
using HangmanApp.ViewModels;
using LottieSharp.WPF;

namespace HangmanApp.Views
{
    public partial class SettingsView : UserControl
    {
        private bool _isMouseDownOnSlider = false;
        private string _tempJsonFile;

        public SettingsView(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // Charger les paramètres au démarrage
            Loaded += UserControl_Loaded;
            LoadJsonResource("sound.json");
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
            // Force an update of the volume setting
            var viewModel = DataContext as SettingsViewModel;
            if (viewModel != null)
            {
                viewModel.MusicVolume = SecureStorage.GetVolume();
            }

            try
            {
                var lottieAnimationView = (LottieAnimationView)this.FindName("LottieSoundAnimationView");
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

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                Point position = e.GetPosition(slider);
                if (position.X >= 0 && position.X <= slider.ActualWidth)
                {
                    double percentage = position.X / slider.ActualWidth;
                    double newValue = percentage * (slider.Maximum - slider.Minimum) + slider.Minimum;
                    slider.Value = newValue;
                    _isMouseDownOnSlider = true;
                }

                e.Handled = true; // Mark the event as handled
            }
        }

        private void Slider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDownOnSlider)
            {
                Slider slider = sender as Slider;
                if (slider != null)
                {
                    Point position = e.GetPosition(slider);
                    if (position.X >= 0 && position.X <= slider.ActualWidth)
                    {
                        double percentage = position.X / slider.ActualWidth;
                        double newValue = percentage * (slider.Maximum - slider.Minimum) + slider.Minimum;
                        slider.Value = newValue;
                    }
                }
            }
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDownOnSlider = false;
        }
    }
}