using CommunityToolkit.Maui.Views;
using SLON.Models;
using SLON.Services;
using SLON.Themes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SLON.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SLON
{
    public partial class Favorites : ContentPage
    {
        // true – Events, false – Profiles
        
        // ����� �����������: Events (true) ��� Profiles (false)
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
            UpdateButtonColors();
        }

        // Изменённый метод OnAppearing: теперь он обновляет данные с сервера перед обновлением UI
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await Models.Favourites.RefreshFavoritesAsync();
                // Предзагружаем mutual пользователей
                var username = AuthService.GetUsernameAsync();
                var mutualUsers = await AuthService.GetMutualUsersAsync(username);
                Favourites.mutual = new ObservableCollection<User>(mutualUsers);
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
            RefreshLikes();
            UpdateButtonColors();
        }

        private void OnPrivateClicked(object sender, EventArgs e)
        {
            showingPublic = false;
            RefreshLikes();
            UpdateButtonColors();
        }

        #region Обработчики переключения
        private void UpdateButtonColors()
        {
            // All / Mutual
            AllButton.SetDynamicResource(Button.BackgroundColorProperty,
                showingAll ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");
            MutualButton.SetDynamicResource(Button.BackgroundColorProperty,
                !showingAll ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");

            // Events / Profiles
            EventsButton.SetDynamicResource(Button.BackgroundColorProperty,
                showingEvents ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");
            ProfilesButton.SetDynamicResource(Button.BackgroundColorProperty,
                !showingEvents ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");

            // Public / Private
            var purple = (Color)Application.Current.Resources["BackGroundColorButtonPurple"];
            var gray = (Color)Application.Current.Resources["BackGroundColorButtonGray"];

            PublicButton.BackgroundColor = showingPublic ? purple : gray;
            PrivateButton.BackgroundColor = showingPublic ? gray : purple;
        }

        private void OnAllClicked(object sender, EventArgs e)
        {
            showingAll = true;
            RefreshLikes();
            UpdateButtonColors(); // Теперь этот метод вызывается автоматически при изменении темы
        }

        private async void OnMutualClicked(object sender, EventArgs e)
        {
            showingAll = false;
            try
            {
                var username = AuthService.GetUsernameAsync();
                var fresh = await AuthService.GetMutualUsersAsync(username);
                Favourites.mutual = new ObservableCollection<User>(fresh);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to refresh mutual: {ex}");
            }
            RefreshLikes();
            UpdateButtonColors();
        }


        private void OnEventsClicked(object sender, EventArgs e)
        {
            showingEvents = true;
            RefreshLikes();
            UpdateButtonColors();
        }

        private void OnProfilesClicked(object sender, EventArgs e)
        {
            showingEvents = false;
            RefreshLikes();
            UpdateButtonColors();
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
                    Console.WriteLine(item.UserData.Username);

                    bool isSuccess = await AuthService.DeleteProfileSwipedUserAsync(item.UserData.Username);

                    if (isSuccess)
                    {
                        Favourites.favorites.Remove(item.UserData);
                        Favourites.mutual.Remove(item.UserData);
                        LikeItems.Remove(item);
                    }
                    else
                        await DisplayAlert("Try again", "Error deleting the user.", "OK");

                }
            }
            else if (!item.IsEvent && item.UserData != null)
            {
                if (showingAll)
                {
                    // как было: удаляем свайп
                    await AuthService.DeleteProfileSwipedUserAsync(item.UserData.Username);
                    Favourites.favorites.Remove(item.UserData);
                }
                else
                {
                    // мы в режиме Mutual — удаляем взаимку
                    bool ok = await AuthService.RemoveMutualUserAsync(AuthService.GetUsernameAsync(), item.UserData.Username);
                    if (!ok)
                    {
                        await DisplayAlert("Error", "Не удалось удалить взаимный лайк", "OK");
                        return;
                    }
                    Favourites.mutual.Remove(item.UserData);
                }
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

                    if (Favourites.mutual == null || Favourites.mutual.Count == 0)
                    {
                        await DisplayAlert("Нет взаимных пользователей", "Ваш список взаимных лайков пуст. Вы не можете добавить никого в это событие.", "OK");
                        return;
                    }

                    var popup = new AddUsersToEventPopup(ev);
                    this.ShowPopup(popup);
                }
                else if (!item.IsEvent && item.UserData != null)
                {
                    if (!IsChatAvailable)
                        return;

                    LinksPopupCtrl.IsEditable = false;
                    await LinksPopupCtrl.Show(item.UserData.Username);
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
                    var profilePage = new Profile();

                    var query = new Dictionary<string, object>
            {
                { "fromPage", "FavoritesPage" },
                { "username", selectedItem.UserData.Username }
            };
                    profilePage.ApplyQueryAttributes(query);

                    await Shell.Current.Navigation.PushAsync(profilePage);
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


    public class LikeItem : INotifyPropertyChanged
    {
        public bool IsEvent { get; set; }
        public Event EventData { get; set; }
        private User _userData;
        public User UserData
        {
            get => _userData;
            set
            {
                if (_userData != value)
                {
                    if (_userData != null)
                    {
                        _userData.PropertyChanged -= UserData_PropertyChanged;
                    }
                    _userData = value;
                    if (_userData != null)
                    {
                        _userData.PropertyChanged += UserData_PropertyChanged;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvatarSource));
                }
            }
        }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string LeftSwipeIcon { get; set; }

        public ImageSource AvatarSource =>
        IsEvent
            ? ImageSource.FromFile("calendar.png")
            : (UserData?.Avatar
               ?? ImageSource.FromFile("default_profile_icon1.png"));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void UserData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(User.Avatar))
            {
                OnPropertyChanged(nameof(AvatarSource));
            }
        }
    }
}
