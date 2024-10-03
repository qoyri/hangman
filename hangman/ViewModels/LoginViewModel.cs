using System;
using System.Windows;
using System.Windows.Input;
using HangmanApp.Services;
using HangmanApp.Views;

namespace HangmanApp.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly AuthService _authService;

        public string Username { get; set; }
        public string Password { get; set; }
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _authService = new AuthService() ?? throw new ArgumentNullException(nameof(_authService));
            LoginCommand = new RelayCommand(Login);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
        }

        private void NavigateToRegister(object parameter)
        {
            _mainViewModel.CurrentViewModel = new RegisterViewModel(_mainViewModel);
        }

        private async void Login(object parameter)
        {
            try
            {
                var token = await _authService.LoginAsync(Username, Password);
                if (!string.IsNullOrEmpty(token))
                {
                    SecureStorage.SaveToken(token);

                    var userInfo = await _authService.GetUserInfoAsync(token);

                    // Mettre à jour le MainViewModel avec les informations de l'utilisateur
                    _mainViewModel.UserName = userInfo.UserName;
                    _mainViewModel.TotalScore = userInfo.TotalScore;
                    _mainViewModel.CurrentWord = userInfo.CurrentWord;
                    _mainViewModel.CurrentWordScore = userInfo.CurrentWordScore;
                    _mainViewModel.ComboMultiplier = userInfo.ComboMultiplier;
                    _mainViewModel.Difficulty = userInfo.Difficulty;

                    await _mainViewModel.UpdateUserInfoAsync();
                    _mainViewModel.CurrentViewModel = new HomeView(new HomeViewModel(_mainViewModel));
                }
                else
                {
                    MessageBox.Show("Invalid credentials!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}