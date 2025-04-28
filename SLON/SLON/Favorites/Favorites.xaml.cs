using CommunityToolkit.Maui.Views;
using SLON.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SLON.Services;

namespace SLON
{
    public partial class Favorites : ContentPage
    {
        // true – Events, false – Profiles
        private bool showingEvents = true;
        // true – All, false – Mutual
        private bool showingAll = true;
        // true – показывать только Public события, false – только Private
        private bool showingPublic = true;
        public static Favorites Instance { get; private set; }

        private ObservableCollection<LikeItem> LikeItems { get; set; } = new();

        public bool IsChatAvailable => !showingEvents && !showingAll;

        public Favorites()
        {
            InitializeComponent();
            BindingContext = this;
            Instance = this;
        }

        // Изменённый метод OnAppearing: теперь он обновляет данные с сервера перед обновлением UI
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await Models.Favourites.RefreshFavoritesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing favorites: {ex.Message}");
            }
            RefreshLikes();
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
        }

        /// <summary>
        /// Обновляет коллекцию лайков и обновляет UI.
        /// </summary>
        public void RefreshLikes()
        {
            LikeItems.Clear();
            OnPropertyChanged(nameof(IsChatAvailable));

            if (showingEvents)
            {
                // вместо скрытия AllMutualStack теперь:
                AllMutualStack.IsVisible = false;
                EventFilterStack.IsVisible = true;

                // фильтруем любимые события по Public/Private
                var filtered = Favourites.favoriteEvents
                    .Where(ev => ev.IsPublic == showingPublic);

                foreach (var ev in filtered)
                {
                    LikeItems.Add(new LikeItem
                    {
                        IsEvent = true,
                        EventData = ev,
                        Title = ev.Name,
                        Subtitle = string.Join(", ", ev.Categories),
                        IconSource = "calendar_icon.png",
                        LeftSwipeIcon = "add_icon2.png"
                    });
                }
            }
            else
            {
                EventFilterStack.IsVisible = false;
                AllMutualStack.IsVisible = true;

                var userCollection = showingAll
                    ? Favourites.favorites.OfType<User>()
                    : Favourites.mutual;

                foreach (var user in userCollection)
                {
                    LikeItems.Add(new LikeItem
                    {
                        IsEvent = false,
                        UserData = user,
                        Title = $"{user.Name} {user.Surname}".Trim(),
                        Subtitle = user.Vocation,
                        IconSource = "default_profile_icon1.png",
                        LeftSwipeIcon = "chat_icon.png"
                    });
                }
            }

            likesCollectionView.ItemsSource = LikeItems;
            UpdateEmptyView();
        }

        private void OnPublicClicked(object sender, EventArgs e)
        {
            showingPublic = true;
            PublicButton.BackgroundColor = Color.FromArgb("#915AC5");
            PrivateButton.BackgroundColor = Colors.DarkGray;
            RefreshLikes();
        }

        private void OnPrivateClicked(object sender, EventArgs e)
        {
            showingPublic = false;
            PrivateButton.BackgroundColor = Color.FromArgb("#915AC5");
            PublicButton.BackgroundColor = Colors.DarkGray;
            RefreshLikes();
        }


        #region Обработчики переключения

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
            // оставляем previous public/private выбор, просто обновляем список
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

        #region Обработчики действий

        private async void OnBellClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RequestsAcceptedPage());
        }

        #endregion

        #region Swipe Handlers
        private async void OnDeleteSwipeInvoked(object sender, EventArgs e)
        {
            if (!(sender is SwipeItem swipeItem && swipeItem.BindingContext is LikeItem item))
                return;

            var username = AuthService.GetUsernameAsync();

            if (item.IsEvent && item.EventData != null)
            {
                var eventHash = item.EventData.Hash;
                bool removedFromSwipes = false;
                bool removedFromMembers = false;

                try
                {
                    removedFromSwipes = await AuthService.DeleteSwipedEventAsync(username, eventHash);
                    Debug.WriteLine($"DeleteSwipedEventAsync returned {removedFromSwipes}");
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"Error deleting swiped event: {ex}");
                }

                try
                {
                    removedFromMembers = await AuthService.RemoveEventMemberAsync(username, eventHash);
                    Debug.WriteLine($"RemoveEventMemberAsync returned {removedFromMembers}");
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"Error removing event member: {ex}");
                }

                if (removedFromSwipes)
                {
                    Favourites.favoriteEvents.Remove(item.EventData);
                    LikeItems.Remove(item);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить событие с сервера.", "OK");
                }
            }
            else if (!item.IsEvent && item.UserData != null)
            {
                Favourites.favorites.Remove(item.UserData);
                Favourites.mutual.Remove(item.UserData);
                LikeItems.Remove(item);
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
                        await DisplayAlert("Event", "Событие не является публичным, добавление пользователей недоступно", "OK");
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
