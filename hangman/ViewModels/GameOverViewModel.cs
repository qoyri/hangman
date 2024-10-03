using System.Windows.Input;

namespace HangmanApp.ViewModels
{
    public class GameOverViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        public ICommand ReturnCommand { get; }

        public GameOverViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            ReturnCommand = new RelayCommand(OnReturn);
        }

        private void OnReturn(object parameter)
        {
            _mainViewModel.CurrentViewModel = new DifficultySelectionViewModel(_mainViewModel);
        }
    }
}