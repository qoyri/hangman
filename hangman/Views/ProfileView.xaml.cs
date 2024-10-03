using System.Windows.Controls;
using HangmanApp.ViewModels;

namespace HangmanApp.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        public ProfileView(ProfileViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}