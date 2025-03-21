using System;
using Microsoft.Maui.Controls;
using SLON.Services;

namespace SLON
{
    public partial class AuthPage : ContentPage
    {
        private bool _isSignUpMode = true;

        public AuthPage()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void UpdateUI()
        {
            TitleLabel.Text = _isSignUpMode ? "HELLO" : "Already registered?";
            SubtitleLabel.Text = _isSignUpMode ? "let's register you!" : "Then sign in!";

            ConfirmPasswordLabel.IsVisible = _isSignUpMode;
            ConfirmPasswordEntry.IsVisible = _isSignUpMode;

            SignUpButton.IsVisible = _isSignUpMode;
            SignInButton.IsVisible = !_isSignUpMode;

            SignInToggle.BackgroundColor = _isSignUpMode ? Colors.Black : Color.FromArgb("#7B51D3");
            SignUpToggle.BackgroundColor = _isSignUpMode ? Color.FromArgb("#7B51D3") : Colors.Black;
        }

        private void ToggleSignInMode(object sender, EventArgs e)
        {
            _isSignUpMode = false;
            UpdateUI();
        }

        private void ToggleSignUpMode(object sender, EventArgs e)
        {
            _isSignUpMode = true;
            UpdateUI();
        }

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            // эти проверки должны быть на сервере
            if (string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please fill all fields.", "OK");
                return;
            }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            //if (!AuthService.IsAuthenticated()) // здесь проверка в базе на корректность введенных данных
            //{
            AuthService.SetAuthenticated(true);
            await AuthService.SaveCredentialsAsync(UsernameEntry.Text, PasswordEntry.Text);
            await DisplayAlert("Registration", "User registered successfully!", "OK");
            Application.Current.MainPage = new AppShell();
            //}
            //else
            //{
                //await DisplayAlert("Registration", "Failed to register user.", "OK");
            //}
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            //if (AuthUser.AuthenticateUser(UsernameEntry.Text, PasswordEntry.Text)) // здесь тоже проверка на корректность и правильность данных 
            //{
            AuthService.SetAuthenticated(true);
            await AuthService.SaveCredentialsAsync(UsernameEntry.Text, PasswordEntry.Text);
            await DisplayAlert("Login", "User logged in successfully!", "OK");
            Application.Current.MainPage = new AppShell();
            //}
            //else
            //{
                //await DisplayAlert("Login", "Failed to log in.", "OK");
            //}
        }
    }
}
