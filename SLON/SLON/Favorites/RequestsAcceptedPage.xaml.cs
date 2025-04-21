using SLON.Models;
using System.Collections.ObjectModel;

namespace SLON
{
    public partial class RequestsAcceptedPage : ContentPage
    {

        bool theme = true;
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
            //#292929
            ThemeRequests();
            ShowRequests();
        }

        public void ThemeRequests()
        {
            RequestsButton.BackgroundColor = Color.FromArgb("#915AC5");
            if (Theme.theme) AcceptedButton.BackgroundColor = Color.FromArgb("#292929");
            else AcceptedButton.BackgroundColor = Colors.DarkGray;
        }

        private void OnAcceptedClicked(object sender, EventArgs e)
        {
            var mp = new MainPage();
            ThemeAccepted();
            ShowAccepted();
        }

        public void ThemeAccepted()
        {
            AcceptedButton.BackgroundColor = Color.FromArgb("#915AC5");
            if (Theme.theme) RequestsButton.BackgroundColor = Color.FromArgb("#292929");
            else RequestsButton.BackgroundColor = Colors.DarkGray;
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
