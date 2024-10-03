using System.Windows.Input;

namespace HangmanApp.ViewModels
{
    public class VictoryControlViewModel : ObservableObject
    {
        public ICommand ContinueCommand { get; }

        public VictoryControlViewModel(ICommand continueCommand)
        {
            ContinueCommand = continueCommand;
        }
    }
}