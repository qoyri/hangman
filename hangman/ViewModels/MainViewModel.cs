using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HangmanApp.Services;
using HangmanApp.Views;

namespace HangmanApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private object _currentViewModel;
        private string _profilePicture;
        private bool _isFullScreen;
        private double _scaleFactor;
        private readonly AuthService _authService;

        public MainViewModel()
        {
            _authService = new AuthService();
            NavigateCommand = new RelayCommand(Navigate);

            // Initialize default values
            UserName = string.Empty;
            TotalScore = 0;
            CurrentWord = string.Empty;
            CurrentWordScore = 0;
            ComboMultiplier = 0.0;
            Difficulty = string.Empty;
            ScaleFactor = 1.0; // Default scale factor

            // Initialize the initial view model
            InitializeAsync();

            // Play the embedded menu music
            MusicService.Instance.PlayMusic("menu_zelda.mp3");
        }

        public double ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = value;
                OnPropertyChanged();
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (_isFullScreen != value)
                {
                    _isFullScreen = value;
                    OnPropertyChanged();
                    ToggleFullScreen(_isFullScreen);
                }
            }
        }

        private void ToggleFullScreen(bool isFullScreen)
        {
            var mainWindow = Application.Current.MainWindow;

            if (isFullScreen)
            {
                mainWindow.WindowStyle = WindowStyle.None;
                mainWindow.WindowState = WindowState.Maximized;
                ScaleFactor = 2.0;
            }
            else
            {
                mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                mainWindow.WindowState = WindowState.Normal;
                ScaleFactor = 1.0;
            }
        }

        public ICommand NavigateCommand { get; }

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        public async Task UpdateUserInfoAsync()
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            string token = SecureStorage.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var userInfo = await _authService.GetUserInfoAsync(token);
                    Console.WriteLine("UserInfo loaded, updating MainViewModel properties");

                    UserName = userInfo.UserName;
                    TotalScore = userInfo.TotalScore;
                    CurrentWord = userInfo.CurrentWord;
                    CurrentWordScore = userInfo.CurrentWordScore;
                    ComboMultiplier = userInfo.ComboMultiplier;
                    Difficulty = userInfo.Difficulty;
                    ProfilePicture = userInfo.ProfilePicture;

                    CurrentViewModel = new HomeView(new HomeViewModel(this));
                }
                catch
                {
                    CurrentViewModel = new LoginViewModel(this);
                }
            }
            else
            {
                CurrentViewModel = new LoginViewModel(this);
            }
        }

        private string _userName;

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private int _totalScore;

        public int TotalScore
        {
            get => _totalScore;
            set
            {
                _totalScore = value;
                OnPropertyChanged();
            }
        }

        private string _currentWord;

        public string CurrentWord
        {
            get => _currentWord;
            set
            {
                _currentWord = value;
                OnPropertyChanged();
            }
        }

        private int _currentWordScore;

        public int CurrentWordScore
        {
            get => _currentWordScore;
            set
            {
                _currentWordScore = value;
                OnPropertyChanged();
            }
        }

        private double _comboMultiplier;

        public double ComboMultiplier
        {
            get => _comboMultiplier;
            set
            {
                _comboMultiplier = value;
                OnPropertyChanged();
            }
        }

        private string _difficulty;

        public string Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                OnPropertyChanged();
            }
        }

        public string ProfilePicture
        {
            get => _profilePicture;
            set
            {
                _profilePicture = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProfilePictureBitmap)); // Notifie ProfilePictureBitmap du changement
            }
        }

        // Propriété ProfilePictureBitmap corrigée
        public BitmapImage ProfilePictureBitmap
        {
            get
            {
                if (string.IsNullOrEmpty(_profilePicture))
                    return null;

                try
                {
                    byte[] imageBytes = Convert.FromBase64String(_profilePicture);
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Freeze to make it cross-thread accessible

                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting profile picture: {ex.Message}");
                    return null;
                }
            }
        }

        private HomeViewModel _homeViewModel;

        public HomeViewModel HomeViewModel
        {
            get => _homeViewModel;
            set
            {
                _homeViewModel = value;
                OnPropertyChanged();
            }
        }

        public void Navigate(object destination)
        {
            Navigate(destination, true); // Default to true for resetCombo
        }

        public void Navigate(object destination, bool resetCombo)
        {
            if (destination == null) return;

            var parts = destination.ToString().Split(';');
            string viewName = parts[0];
            string difficulty = parts.Length > 1 ? parts[1] : Difficulty;

            switch (viewName)
            {
                case "Login":
                    MusicService.Instance.StopMusic();
                    CurrentViewModel = new LoginViewModel(this);
                    break;
                case "Home":
                    CurrentViewModel = new HomeView(new HomeViewModel(this));
                    break;
                case "SelectDifficulty":
                    CurrentViewModel = new DifficultySelectionView(new DifficultySelectionViewModel(this));
                    break;
                case "GameView":
                    if (!string.IsNullOrEmpty(difficulty))
                    {
                        Difficulty = difficulty;
                        if (resetCombo)
                        {
                            ComboMultiplier = 1.0; // Reset the combo only if resetCombo is true
                        }

                        GameViewModel gameViewModel = new GameViewModel(this, difficulty);
                        CurrentViewModel = new GameView(gameViewModel, this);
                    }
                    else
                    {
                        throw new ArgumentException("Difficulty cannot be null or empty for GameView.");
                    }

                    break;
                case "Settings":
                    var settingsViewModel = new SettingsViewModel(this);
                    var settingsView = new SettingsView(settingsViewModel);
                    CurrentViewModel = settingsView;
                    break;
                case "Profile":
                    var profileViewModel = new ProfileViewModel(this);
                    var profileView = new ProfileView(profileViewModel);
                    CurrentViewModel = profileView;
                    break;
                case "Ranking":
                    var rankingView = new RankingView();
                    var rankingViewModel = new RankingViewModel(this);
                    rankingView.DataContext = rankingViewModel;
                    CurrentViewModel = rankingView; // Associez ViewModel au DataContext
                    break;
                default:
                    throw new ArgumentException($"Unknown destination: {destination}");
            }
        }
    }
}