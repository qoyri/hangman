using System;
using System.Windows.Input;

namespace HangmanApp.ViewModels
{
    public class HintOverlayViewModel : ObservableObject
    {
        private string _hintImageSource;
        public ICommand CloseHintCommand { get; }

        public string HintImageSource
        {
            get => _hintImageSource;
            set
            {
                _hintImageSource = value;
                OnPropertyChanged();
            }
        }

        public HintOverlayViewModel(ICommand closeHintCommand)
        {
            CloseHintCommand = closeHintCommand ?? throw new ArgumentNullException(nameof(closeHintCommand));
        }
    }
}