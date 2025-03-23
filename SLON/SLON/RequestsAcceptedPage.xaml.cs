using SLON.Models;
using System.Collections.ObjectModel;

namespace SLON
{
    public partial class RequestsAcceptedPage : ContentPage
    {
        private ObservableCollection<User> requestsList = new();
        private ObservableCollection<User> acceptedList = new();

        // Привязка для кнопок Accept/Decline
        public bool IsRequestsMode { get; set; } = true;

        public RequestsAcceptedPage()
        {
            InitializeComponent();
            BindingContext = this;

            foreach (var user in Favourites.requests)
                requestsList.Add(user);
            foreach (var user in Favourites.accepted)
                acceptedList.Add(user);

            ShowRequests();
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnRequestsClicked(object sender, EventArgs e)
        {
            RequestsButton.BackgroundColor = Color.FromArgb("#915AC5");
            AcceptedButton.BackgroundColor = Colors.DarkGray;
            ShowRequests();
        }

        private void OnAcceptedClicked(object sender, EventArgs e)
        {
            AcceptedButton.BackgroundColor = Color.FromArgb("#915AC5");
            RequestsButton.BackgroundColor = Colors.DarkGray;
            ShowAccepted();
        }

        private void ShowRequests()
        {
            IsRequestsMode = true;
            OnPropertyChanged(nameof(IsRequestsMode));
            UsersCollectionView.ItemsSource = requestsList;
        }

        private void ShowAccepted()
        {
            IsRequestsMode = false;
            OnPropertyChanged(nameof(IsRequestsMode));
            UsersCollectionView.ItemsSource = acceptedList;
        }

        private void OnDeclineClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is User user)
            {
                requestsList.Remove(user);
                Favourites.requests.Remove(user);
            }
        }

        private void OnAcceptClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is User user)
            {
                user.IsMutual = true;
                requestsList.Remove(user);
                Favourites.requests.Remove(user);

                if (!Favourites.mutual.Any(u => u.Id == user.Id))
                    Favourites.mutual.Add(user);
            }
        }

        private void OnUserTapped(object sender, EventArgs e)
        {
            if (sender is StackLayout layout && layout.BindingContext is User user)
            {
                DisplayAlert("Profile Tapped", $"Нажат профиль: {user.Name}", "OK");
            }
        }
    }
}
