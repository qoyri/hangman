using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using HangmanApp.ViewModels;

namespace HangmanApp.Views
{
    public partial class DifficultySelectionView : UserControl
    {
        public DifficultySelectionView(DifficultySelectionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        public DifficultySelectionView()
        {
            InitializeComponent();
        }
        
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["UserControlLoadAnimation"];
            storyboard.Begin();
        }

        private async void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["UserControlUnloadAnimation"];
    
            // Utiliser une TaskCompletionSource pour attendre la fin de l'animation
            var tcs = new TaskCompletionSource<bool>();
            storyboard.Completed += (s, _) => tcs.SetResult(true);

            storyboard.Begin();
            await tcs.Task; // Attendre la fin de l'animation
        }


    }
}