using System;
using System.Windows.Input;
using HangmanApp.Services;

namespace HangmanApp.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private double _musicVolume;
        private bool _isFullScreen;
        private readonly MainViewModel _mainViewModel;
        public ICommand ReturnCommand { get; }

        public SettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            ReturnCommand = new RelayCommand(OnReturn);
            _isFullScreen = _mainViewModel.IsFullScreen;

            // Charger le volume de musique au démarrage
            _musicVolume = SecureStorage.GetVolume();
            MusicService.Instance.SetVolume(_musicVolume);
        }

        public double MusicVolume
        {
            get => _musicVolume;
            set
            {
                if (_musicVolume != value && value >= 0 && value <= 1)
                {
                    _musicVolume = value;
                    OnPropertyChanged(nameof(MusicVolume));
                    MusicService.Instance.SetVolume(value);
                    SecureStorage.SaveVolume(value);
                }
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
                    OnPropertyChanged(nameof(IsFullScreen));
                    _mainViewModel.IsFullScreen = value;
                }
            }
        }

        private void OnReturn(object parameter)
        {
            _mainViewModel.NavigateCommand.Execute("Home");
        }
    }
}