using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HangmanApp.Services;
using HangmanApp.Managers;

namespace HangmanApp.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        public ICommand StartGameCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenProfileCommand { get; }
        public ICommand OpenRankingCommand { get; }

        private readonly MainViewModel _mainViewModel;

        public HomeViewModel()
        {
            Console.WriteLine("HomeViewModel default constructor called");
            StartGameCommand = new RelayCommand(StartGame);
            LogoutCommand = new RelayCommand(Logout);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenProfileCommand = new RelayCommand(OpenProfile);
            OpenRankingCommand = new RelayCommand(OpenRanking);
        }

        public HomeViewModel(MainViewModel mainViewModel) : this()
        {
            Console.WriteLine("HomeViewModel constructor with MainViewModel called");
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            OnPropertyChanged(nameof(ProfilePictureBitmap));
        }

        public BitmapImage ProfilePictureBitmap
        {
            get
            {
                if (_mainViewModel == null)
                {
                    Console.WriteLine("Error: _mainViewModel is null in HomeViewModel. Returning default image.");
                    return LoadDefaultProfilePicture();
                }

                var profilePicture = _mainViewModel.ProfilePicture;
                if (string.IsNullOrEmpty(profilePicture))
                {
                    Console.WriteLine("ProfilePicture is empty or null. Loading default image.");
                    return LoadDefaultProfilePicture();
                }
                else
                {
                    try
                    {
                        Console.WriteLine("Loading user profile picture from Base64");
                        byte[] imageBytes = Convert.FromBase64String(profilePicture);
                        using (var stream = new MemoryStream(imageBytes))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            Console.WriteLine(
                                $"Loaded user image. Dimensions before resize: {bitmap.PixelWidth}x{bitmap.PixelHeight}");

                            if (bitmap.PixelWidth > 1024 || bitmap.PixelHeight > 1024)
                            {
                                bitmap = ResizeImage(bitmap, 1024, 1024);
                                Console.WriteLine(
                                    $"Resized user image. New Dimensions: {bitmap.PixelWidth}x{bitmap.PixelHeight}");
                            }

                            return bitmap;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading user profile picture: {ex.Message}");
                        return LoadDefaultProfilePicture();
                    }
                }
            }
        }

        private BitmapImage LoadDefaultProfilePicture()
        {
            return ImageService.Instance.LoadImage("default_profile_pic.jpg");
        }

        private BitmapImage ResizeImage(BitmapImage originalImage, int maxWidth, int maxHeight)
        {
            int originalWidth = originalImage.PixelWidth;
            int originalHeight = originalImage.PixelHeight;

            double ratioX = (double)maxWidth / originalWidth;
            double ratioY = (double)maxHeight / originalHeight;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            var resizedBitmap = new TransformedBitmap(originalImage, new ScaleTransform(ratio, ratio));

            using (var memoryStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(resizedBitmap));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                var resultImage = new BitmapImage();
                resultImage.BeginInit();
                resultImage.StreamSource = memoryStream;
                resultImage.CacheOption = BitmapCacheOption.OnLoad;
                resultImage.EndInit();
                resultImage.Freeze();

                return resultImage;
            }
        }

        private void StartGame(object parameter)
        {
            if (_mainViewModel == null)
            {
                MessageBox.Show("Erreur : MainViewModel est nul. Impossible de démarrer le jeu.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _mainViewModel.NavigateCommand.Execute("SelectDifficulty;easy");
        }

        private void Logout(object parameter)
        {
            if (_mainViewModel == null)
            {
                MessageBox.Show("Erreur : MainViewModel est nul. Déconnexion impossible.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SecureStorage.ClearToken();
            MessageBox.Show("Déconnexion réussie.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);

            // Arrêter la musique du menu et passer à la page de connexion
            MusicManager.Instance.StopMusic();
            _mainViewModel.NavigateCommand.Execute("Login;none");
        }

        private void OpenSettings(object parameter)
        {
            if (_mainViewModel == null)
            {
                MessageBox.Show("Erreur : MainViewModel est nul. Impossible d'ouvrir les paramètres.", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _mainViewModel.NavigateCommand.Execute("Settings;none");
        }

        private void OpenProfile(object parameter)
        {
            _mainViewModel.Navigate("Profile");
        }

        private void OpenRanking(object parameter)
        {
            _mainViewModel.Navigate("Ranking");
        }
    }
}