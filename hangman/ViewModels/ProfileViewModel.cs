using HangmanApp.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HangmanApp.ViewModels
{
    public class ProfileViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly AuthService _authService;

        public ProfileViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _authService = new AuthService();
            BackCommand = new RelayCommand(Back);
            UploadCommand = new RelayCommand(UploadProfilePicture);
        }

        public ICommand BackCommand { get; }
        public ICommand UploadCommand { get; }

        public string UserName => _mainViewModel.UserName;
        public int TotalScore => _mainViewModel.TotalScore;
        public BitmapImage ProfilePictureBitmap => _mainViewModel.ProfilePictureBitmap;

        private void Back(object parameter)
        {
            _mainViewModel.Navigate("Home");
        }

        private async void UploadProfilePicture(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                Title = "Select Profile Picture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                if (fileInfo.Length > 2 * 1024 * 1024) // 2MB limit
                {
                    MessageBox.Show("File size exceeds 2MB. Please select a smaller file.", "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    byte[] fileBytes;
                    using (var stream = new MemoryStream())
                    {
                        using (var fileStream = File.OpenRead(openFileDialog.FileName))
                        {
                            await fileStream.CopyToAsync(stream);
                        }
                        fileBytes = stream.ToArray();
                    }

                    // Supposons que le token d'authentification est stocké dans la vue modèle principal
                    var token = SecureStorage.GetToken();
                    var response = await _authService.UploadProfilePicture(fileBytes, openFileDialog.FileName, token);

                    if (response)
                    {
                        string base64String = Convert.ToBase64String(fileBytes);
                        _mainViewModel.ProfilePicture = base64String;
                        await _mainViewModel.UpdateUserInfoAsync();
                        MessageBox.Show("Profile picture updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to upload profile picture. Check logs for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error uploading profile picture: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}