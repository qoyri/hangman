using System;
using System.Windows.Input;
using HangmanApp.Views;

namespace HangmanApp.ViewModels
{
    public class DifficultySelectionViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public ICommand EasyCommand { get; }
        public ICommand MediumCommand { get; }
        public ICommand HardCommand { get; }
        public ICommand ReturnCommand { get; }

        public DifficultySelectionViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            EasyCommand = new RelayCommand(StartGameWithEasy);
            MediumCommand = new RelayCommand(StartGameWithMedium);
            HardCommand = new RelayCommand(StartGameWithHard);
            ReturnCommand = new RelayCommand(OnReturn);
        }

        private void StartGameWithEasy(object parameter)
        {
            _mainViewModel.NavigateCommand.Execute("GameView;easy");
        }

        private void StartGameWithMedium(object parameter)
        {
            _mainViewModel.NavigateCommand.Execute("GameView;medium");
        }

        private void StartGameWithHard(object parameter)
        {
            _mainViewModel.NavigateCommand.Execute("GameView;hard");
        }

        private void OnReturn(object parameter)
        {
            _mainViewModel.NavigateCommand.Execute("Home");
        }
    }
}