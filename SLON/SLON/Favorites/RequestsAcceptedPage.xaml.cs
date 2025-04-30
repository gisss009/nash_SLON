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

        // Accept/Decline
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

        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnRequestsClicked(object sender, EventArgs e)
        {
            // Выделяем кнопку Requests
            RequestsButton.BackgroundColor = Color.FromArgb("#915AC5");
            AcceptedButton.BackgroundColor = Colors.DarkGray;

            // Переключаем режим на запросы
            IsRequestsMode = true;
            OnPropertyChanged(nameof(IsRequestsMode));

            // Показываем список входящих запросов
            UsersCollectionView.ItemsSource = requestsList;
        }


        private void OnAcceptedClicked(object sender, EventArgs e)
        {
            AcceptedButton.BackgroundColor = Color.FromArgb("#915AC5");
            RequestsButton.BackgroundColor = Colors.DarkGray;
            IsRequestsMode = false;
            OnPropertyChanged(nameof(IsRequestsMode));

            // вместо acceptedList используем из Favourites
            UsersCollectionView.ItemsSource = Favourites.mutual;
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

                    Console.WriteLine($"{user.Username} ��� declined.");

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
                if (!isSuccess)
                {
                    await DisplayAlert("Error", "Не удалось принять запрос", "OK");
                    return;
                }

                // 1) удаляем из списка запросов и из All
                requestsList.Remove(user);
                Favourites.requests.Remove(user);

                // 2) обновляем mutual из сервера
                var username = AuthService.GetUsernameAsync();
                var newMutual = await AuthService.GetMutualUsersAsync(username);
                Favourites.mutual = new ObservableCollection<User>(newMutual);

                // 3) если сейчас открыта вкладка Accepted — обновляем её
                if (!IsRequestsMode)
                {
                    UsersCollectionView.ItemsSource = Favourites.mutual;
                }

                await DisplayAlert("Успех", $"Вы и {user.FullName} теперь взаимные лайки", "OK");
            }
        }


        private async void OnUserTapped(object sender, EventArgs e)
        {
            if (sender is StackLayout layout && layout.BindingContext is User user)
            {
                var profilePage = new Profile();

                var query = new Dictionary<string, object>
                    {
                        { "fromPage", "FavoritesPage" },
                        { "username", user.Username }
                    };
                profilePage.ApplyQueryAttributes(query);

                await Shell.Current.Navigation.PushAsync(profilePage);
            }
        }
    }
}