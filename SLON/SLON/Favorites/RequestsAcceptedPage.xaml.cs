using SLON.Models;
using System.Collections.ObjectModel;

namespace SLON
{
    public partial class RequestsAcceptedPage : ContentPage
    {

        bool theme = true;
        private ObservableCollection<User> requestsList = new();
        private ObservableCollection<User> acceptedList = new();

        // �������� ��� ������ Accept/Decline
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
            // ������������� ������������ ������� ������ ����������� ������
            RequestsButton.SetDynamicResource(
                Button.BackgroundColorProperty,
                "BackGroundColorButtonPurple" // �������� ������ (����������)
            );
            AcceptedButton.SetDynamicResource(
                Button.BackgroundColorProperty,
                "BackGroundColorButtonGray" // ���������� ������ (�����)
            );
        }

        private void OnAcceptedClicked(object sender, EventArgs e)
        {
            var mp = new MainPage();
            ThemeAccepted();
            ShowAccepted();
        }

        public void ThemeAccepted()
{
    // ������������� ������������ ������� ������ ����������� ������
    AcceptedButton.SetDynamicResource(
        Button.BackgroundColorProperty, 
        "BackGroundColorButtonPurple" // �������� ������ (����������)
    );
    RequestsButton.SetDynamicResource(
        Button.BackgroundColorProperty, 
        "BackGroundColorButtonGray" // ���������� ������ (�����)
    );
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
                DisplayAlert("Profile Tapped", $"����� �������: {user.Name}", "OK");
            }
        }
    }
}
