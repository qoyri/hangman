using System;
using System.Windows;
using System.Windows.Controls;
using HangmanApp.ViewModels;

namespace HangmanApp.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public LoginView(MainViewModel mainViewModel) : this()
        {
            if (mainViewModel == null)
            {
                throw new ArgumentNullException(nameof(mainViewModel),
                    "MainViewModel is null in LoginView constructor");
            }

            DataContext = mainViewModel; // Use the MainViewModel as DataContext
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}