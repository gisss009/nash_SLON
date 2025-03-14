using CommunityToolkit.Maui.Views;
using SLON.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SLON
{
<<<<<<< Updated upstream
    public ObservableCollection<User> favorites = Favourites.favorites;
    private Image starIcon;

    public Favorites()
=======
    public partial class Favorites : ContentPage
>>>>>>> Stashed changes
    {
        // Режим отображения: Events (true) или Profiles (false)
        private bool showingEvents = true;
        // Для профилей: All (true) или Mutual (false)
        private bool showingAll = true;

        private ObservableCollection<LikeItem> LikeItems { get; set; } = new();

        public bool IsChatAvailable => !showingEvents && !showingAll;

<<<<<<< Updated upstream
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        Debug.WriteLine($"count posle: {favorites.Count}");
        favoritesPanel.Children.Clear();
        //Image starIcon;
        if (favorites.Count == 0)
=======
        public Favorites()
>>>>>>> Stashed changes
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshLikes();
        }

        private void RefreshLikes()
        {
            LikeItems.Clear();
            OnPropertyChanged(nameof(IsChatAvailable));

            if (showingEvents)
            {
                AllMutualStack.IsVisible = false;

                foreach (var ev in Favourites.favoriteEvents)
                {
                    LikeItems.Add(new LikeItem
                    {
                        IsEvent = true,
                        EventData = ev,
                        Title = ev.Name,
                        Subtitle = ev.Categories,
                        IconSource = "calendar_icon.png",
                        LeftSwipeIcon = "add_icon2.png"
                    });
                }
            }
            else
            {
                AllMutualStack.IsVisible = true;
                var userCollection = showingAll ? Favourites.favorites.OfType<User>() : Favourites.mutual;

                foreach (var user in userCollection)
                {
                    LikeItems.Add(new LikeItem
                    {
                        IsEvent = false,
                        UserData = user,
                        Title = user.Name,
                        Subtitle = user.Vocation,
                        IconSource = "default_profile_icon1.png",
                        LeftSwipeIcon = "chat_icon.png"
                    });
                }
            }

            likesCollectionView.ItemsSource = LikeItems;
            UpdateEmptyView();
        }



        #region Переключатели

        private void OnAllClicked(object sender, EventArgs e)
        {
            showingAll = true;
            AllButton.BackgroundColor = Color.FromArgb("#915AC5");
            MutualButton.BackgroundColor = Colors.DarkGray;
            RefreshLikes();
        }

        private void OnMutualClicked(object sender, EventArgs e)
        {
            showingAll = false;
            MutualButton.BackgroundColor = Color.FromArgb("#915AC5");
            AllButton.BackgroundColor = Colors.DarkGray;
            RefreshLikes();
        }

        private void OnEventsClicked(object sender, EventArgs e)
        {
            showingEvents = true;
            EventsButton.BackgroundColor = Color.FromArgb("#915AC5");
            ProfilesButton.BackgroundColor = Colors.DarkGray;
            RefreshLikes();
        }

        private void OnProfilesClicked(object sender, EventArgs e)
        {
            showingEvents = false;
            ProfilesButton.BackgroundColor = Color.FromArgb("#915AC5");
            EventsButton.BackgroundColor = Colors.DarkGray;
            RefreshLikes();
        }

        #endregion

        #region Колокольчик

        private async void OnBellClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RequestsAcceptedPage());
        }

        #endregion

        #region Swipe Handlers

        private void OnDeleteSwipeInvoked(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is LikeItem item)
            {
                if (item.IsEvent && item.EventData != null)
                {
<<<<<<< Updated upstream
                    Text = $"{user.Name}\n{user.Vocation}",
                    TextColor = Colors.White,
                    FontSize = 16,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start
                };
                Grid.SetColumn(userLabel, 1);

                starIcon = new Image
                {
                    Source = ImageSource.FromFile("Resources/Images/empty_star.png"),
                    WidthRequest = 50,
                    HeightRequest = 50,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End
                };
                Grid.SetColumn(starIcon, 1);
                

                userGrid.Children.Add(userIcon);
                userGrid.Children.Add(userLabel);
                userGrid.Children.Add(starIcon);

                TapGestureRecognizer tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnUserTapped(user);
                userGrid.GestureRecognizers.Add(tapGesture);

                userFrame.Content = userGrid;

                favoritesPanel.Children.Add(userFrame);
            }
        }
    }

    private async void OnUserTapped(User selectedUser)
    {
        // Получаем starIcon через родительский элемент, чтобы изменить его источник
        var userFrame = favoritesPanel.Children
            .OfType<Frame>()
            .FirstOrDefault(f => f.Content is Grid userGrid && userGrid.Children.OfType<Label>().Any(l => l.Text.Contains(selectedUser.Name)));

        if (userFrame != null)
        {
            var userGrid = (Grid)userFrame.Content;
            var starIcon = userGrid.Children.OfType<Image>().LastOrDefault(); // Получаем последний элемент, который является starIcon

            if (starIcon != null)
            {
                // Меняем источник изображения
                if (starIcon.Source.ToString().Contains("empty_star.png"))
                {
                    starIcon.Source = ImageSource.FromFile("Resources/Images/fill_star.png");
                }
                else
                {
                    starIcon.Source = ImageSource.FromFile("Resources/Images/empty_star.png");
                }
            }
        }

        //await DisplayAlert("User Info", $"You clicked on {selectedUser.Name}", "OK");
    }

    //private async void OnUserTapped(User selectedUser)
    //{

    //    await DisplayAlert("User Info", $"You clicked on {selectedUser.Name}", "OK");
    //}
};
=======
                    Favourites.favoriteEvents.Remove(item.EventData);
                    LikeItems.Remove(item);
                }
                else if (!item.IsEvent && item.UserData != null)
                {
                    Favourites.favorites.Remove(item.UserData);
                    Favourites.mutual.Remove(item.UserData);
                    LikeItems.Remove(item);
                }
            }
        }

        private async void OnRightSwipeInvoked(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is LikeItem item)
            {
                if (item.IsEvent && item.EventData != null)
                {
                    var ev = item.EventData;
                    if (!ev.IsPublic)
                    {
                        await DisplayAlert("Event", "Ивент приватный, нельзя добавить участников", "OK");
                        return;
                    }
                    var popup = new AddUsersToEventPopup(ev);
                    this.ShowPopup(popup);
                }
                else if (!item.IsEvent && item.UserData != null)
                {
                    if (!IsChatAvailable)
                        return;
                    await DisplayAlert("Chat", $"Открыт чат с {item.Title}", "OK");
                }
            }
        }
>>>>>>> Stashed changes

        private async void OnItemTapped(object sender, EventArgs e)
        {
            if (sender is BindableObject bindable && bindable.BindingContext is LikeItem selectedItem)
            {
                if (selectedItem.IsEvent && selectedItem.EventData != null)
                {
                    var popup = new EventReadOnlyPopup(selectedItem.EventData);
                    this.ShowPopup(popup);
                }
                else if (!selectedItem.IsEvent && selectedItem.UserData != null)
                {
                    await DisplayAlert("User Info", $"Просмотр профиля: {selectedItem.UserData.Name}", "OK");
                }
            }
        }

        private void UpdateEmptyView()
        {
            bool isEmpty = LikeItems == null || LikeItems.Count == 0;
            EmptyViewLayout.IsVisible = isEmpty;
            likesCollectionView.IsVisible = !isEmpty;
        }


        #endregion
    }

    public class LikeItem
    {
        public bool IsEvent { get; set; }
        public Event EventData { get; set; }
        public User UserData { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string IconSource { get; set; }
        public string LeftSwipeIcon { get; set; }
    }
}
