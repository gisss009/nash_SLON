using CommunityToolkit.Maui.Views;
using SLON.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SLON
{
    public partial class Favorites : ContentPage
    {
        private bool showingEvents = true;
        private bool showingAll = true;

        private ObservableCollection<LikeItem> LikeItems { get; set; } = new();

        public bool IsChatAvailable => !showingEvents && !showingAll;

        public Favorites()
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
                        Subtitle = string.Join(", ", ev.Categories), // Преобразование списка в строку
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

        private async void OnBellClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RequestsAcceptedPage());
        }

        private void OnDeleteSwipeInvoked(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is LikeItem item)
            {
                if (item.IsEvent && item.EventData != null)
                {
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
                        await DisplayAlert("Event", "Это приватное событие, добавление невозможно", "OK");
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
                    await DisplayAlert("User Info", $"Информация о пользователе: {selectedItem.UserData.Name}", "OK");
                }
            }
        }

        private void UpdateEmptyView()
        {
            bool isEmpty = LikeItems == null || LikeItems.Count == 0;
            EmptyViewLayout.IsVisible = isEmpty;
            likesCollectionView.IsVisible = !isEmpty;
        }
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
