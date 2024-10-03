using System;
using System.Windows.Controls;
using System.Windows.Input;
using HangmanApp.ViewModels;

namespace HangmanApp.Views
{
    public partial class GameView : UserControl
    {
        private GameViewModel _viewModel;

        public GameView(GameViewModel viewModel, MainViewModel mainViewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

            this.Loaded += GameView_Loaded;

            // Attach key down event handler
            this.KeyDown += GameView_KeyDown;
        }

        private void GameView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Focus();
        }

        private void GameView_KeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel.IsVictoryOverlayVisible)
            {
                if (e.Key == Key.Space)
                {
                    _viewModel.ContinueCommand.Execute(null);
                }

                // Ignore other key events if victory overlay is visible
                return;
            }

            if (e.Key == Key.Escape)
            {
                _viewModel.ReturnCommand.Execute(null);
                return;
            }

            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                char guessedLetter = char.ToUpper((char)('A' + (e.Key - Key.A)));
                _viewModel.GuessCommand.Execute(guessedLetter);
            }
            else if (e.Key == Key.F1)
            {
                // Trigger hint when the Up arrow key is pressed
                _viewModel.ShowHintCommand.Execute(null);
            }
            else if (e.Key == Key.F2)
            {
                // Hide hint overlay when the Down arrow key is pressed
                _viewModel.CloseHintOverlay(null);
            }
        }
    }
}