using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using HangmanApp.ViewModels;
using MahApps.Metro.Controls;

namespace HangmanApp.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.Resources["WindowLoadAnimation"];
            storyboard.Begin();
        }
    }
}