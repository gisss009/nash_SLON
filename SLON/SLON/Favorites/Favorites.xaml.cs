using CommunityToolkit.Maui.Views;
using SLON.Models;
using SLON.Themes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SLON
{
    public partial class Favorites : ContentPage
    {
        
        // ����� �����������: Events (true) ��� Profiles (false)
        private bool showingEvents = true;
        // ��� ��������: All (true) ��� Mutual (false)
        private bool showingAll = true;

        private ObservableCollection<LikeItem> LikeItems { get; set; } = new();

        public bool IsChatAvailable => !showingEvents && !showingAll;

        public Favorites()
        {
            InitializeComponent();
            UpdateButtonColors();

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshLikes();
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            

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
                        IconSource = "default_profile_icon2.png",
                        LeftSwipeIcon = "chat_icon.png"
                    });
                }
            }

            likesCollectionView.ItemsSource = LikeItems;
            UpdateEmptyView();
        }

        #region �������������
        private void UpdateButtonColors()
        {
            // Для кнопок All/Mutual
            AllButton.SetDynamicResource(Button.BackgroundColorProperty,
                showingAll ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");
            MutualButton.SetDynamicResource(Button.BackgroundColorProperty,
                !showingAll ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");

            // Для кнопок Events/Profiles
            EventsButton.SetDynamicResource(Button.BackgroundColorProperty,
                showingEvents ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");
            ProfilesButton.SetDynamicResource(Button.BackgroundColorProperty,
                !showingEvents ? "BackGroundColorButtonPurple" : "BackGroundColorButtonGray");
        }
        private void OnAllClicked(object sender, EventArgs e)
        {
            showingAll = true;
            RefreshLikes();
            UpdateButtonColors(); // Теперь этот метод вызывается автоматически при изменении темы
        }

        private void OnMutualClicked(object sender, EventArgs e)
        {
            showingAll = false;
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

        #region �����������

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
                        await DisplayAlert("Event", "����� ���������, ������ �������� ����������", "OK");
                        return;
                    }
                    var popup = new AddUsersToEventPopup(ev);
                    this.ShowPopup(popup);
                }
                else if (!item.IsEvent && item.UserData != null)
                {
                    if (!IsChatAvailable)
                        return;
                    await DisplayAlert("Chat", $"������ ��� � {item.Title}", "OK");
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
                    await DisplayAlert("User Info", $"�������� �������: {selectedItem.UserData.Name}", "OK");
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
