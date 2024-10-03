using System;
using System.Windows.Input;
using HangmanApp.Services;

namespace HangmanApp.ViewModels
{
    public class RegisterViewModel : ObservableObject
    {
        private string _username;
        private string _password;
        private string _confirmPassword;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUsernameEmpty));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPasswordEmpty));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsConfirmPasswordEmpty));
            }
        }

        public bool IsUsernameEmpty => string.IsNullOrWhiteSpace(Username);
        public bool IsPasswordEmpty => string.IsNullOrWhiteSpace(Password);
        public bool IsConfirmPasswordEmpty => string.IsNullOrWhiteSpace(ConfirmPassword);

        public string ErrorMessage { get; set; }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        private readonly MainViewModel _mainViewModel;
        private readonly AuthService _authService;

        public RegisterViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _authService = new AuthService() ?? throw new ArgumentNullException(nameof(_authService));
            RegisterCommand = new RelayCommand(Register);
            NavigateToLoginCommand =
                new RelayCommand(param => _mainViewModel.CurrentViewModel = new LoginViewModel(_mainViewModel));
        }

        private async void Register(object parameter)
        {
            try
            {
                var result = await _authService.RegisterAsync(Username, Password, ConfirmPassword);
                if (result)
                {
                    _mainViewModel.CurrentViewModel = new LoginViewModel(_mainViewModel);
                }
                else
                {
                    ErrorMessage = "Registration failed.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
    }
}