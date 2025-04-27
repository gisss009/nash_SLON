using CommunityToolkit.Maui.Core.Extensions;
using SLON.Models;
using SLON.Services;
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

            ShowRequests();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            requestsList.Clear();
            acceptedList.Clear();

            var reqs = await AuthService.GetRequestsUsersAsync();
            foreach (var u in reqs)
                requestsList.Add(u);

            var accs = await AuthService.GetAcceptedUsersDataAsync();
            foreach (var user in accs)
            {
                var u = new User(
                    username: user.username,
                    name: user.name,
                    surname: user.surname,
                    tags: default,
                    vocation: user.vocation,
                    info: default,
                    skills: default
                );
                acceptedList.Add(u);
            }

            if (IsRequestsMode)
                UsersCollectionView.ItemsSource = requestsList;
            else
                UsersCollectionView.ItemsSource = acceptedList;
        }

        //protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        //{
        //    base.OnNavigatedTo(args);

        //    List<User> reqs = await AuthService.GetRequestsUsersAsync();
        //    Favourites.requests = reqs.ToObservableCollection();

        //    List<AuthService.UserData> accss = await AuthService.GetAcceptedUsersDataAsync();

        //    foreach (AuthService.UserData user in accss)
        //    {
        //        if (Favourites.accepted.Count(x => x.Username == user.username) > 0)
        //            continue;

        //        Favourites.accepted.Add(new User(
        //            username: user.username,
        //            name: user.name + user.surname,
        //            tags: default,
        //            vocation: user.vocation,
        //            info: default,
        //            skills: default
        //            ));
        //    }


        //}

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

        private async void OnDeclineClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is User user)
            {
                bool isSuccess = await AuthService.DeclineUserAsync(user.Username);

                if (isSuccess)
                {
                    requestsList.Remove(user);
                    Favourites.requests.Remove(user);

                    Console.WriteLine($"{user.Username} был declined.");

                }
                else
                {
                    await DisplayAlert("Error :(", $"An error occurred during acceptance", "OK");
                }
            }
        }

        private async void OnAcceptClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is User user)
            {
                bool isSuccess = await AuthService.AcceptUserAsync(user.Username);

                if (isSuccess)
                {
                    requestsList.Remove(user);
                    Favourites.requests.Remove(user);

                    if (!Favourites.mutual.Any(u => u.Username == user.Username))
                        Favourites.mutual.Add(user);

                    Console.WriteLine($"{user.Username} был добавлен в Accepted");
                }
                else
                {
                    await DisplayAlert("Error :(", $"An error occurred during acceptance", "OK");
                }

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
