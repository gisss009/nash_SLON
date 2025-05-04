using CommunityToolkit.Maui.Views;
using Plugin.Maui.SwipeCardView.Core;
using SLON.Models;
using SLON.Themes;
using SLON.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Globalization;
using System.ComponentModel;
using Microsoft.Maui.Layouts;
using System.Runtime.CompilerServices;

namespace SLON
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Event> Events { get; set; } = new();
        public ICommand OnCardSwipedCommand { get; }
        // 0 - профили, 1 - ивенты
        public int ProfilesEventsButtonStatus = 0;
        private List<Event> _allEventsCache;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        // Флаги пустого состояния
        public bool IsUsersEmpty =>
            ProfilesEventsButtonStatus == 0
            && Users.Count == 0;

        public bool IsEventsEmpty =>
            ProfilesEventsButtonStatus == 1
            && Events.Count == 0;

        public MainPage()
        {
            InitializeComponent();

            OnCardSwipedCommand = new Command<SwipedCardEventArgs>(OnCardSwiped);
            BindingContext = this;

            Shell.SetNavBarIsVisible(this, false);

            swipeCardView.Dragging += OnDragging;
            swipeCardViewEvent.Dragging += OnDragging;

            // подписываемся на изменения коллекций
            Users.CollectionChanged += (_, __) => RaiseEmptyFlags();
            Events.CollectionChanged += (_, __) => RaiseEmptyFlags();
        }

        void RaiseEmptyFlags()
        {
            OnPropertyChanged(nameof(IsUsersEmpty));
            OnPropertyChanged(nameof(IsEventsEmpty));
        }

        private void OnDragging(object sender, DraggingCardEventArgs e)
        {
            var defaultCardColor = (Color)Application.Current.Resources["CardBackgroundColor"];

            if (e.Item is User user)
            {
                switch (e.Position)
                {
                    case DraggingCardPosition.OverThreshold:
                        user.CardColor = e.Direction == SwipeCardDirection.Left
                            ? Colors.Green
                            : Colors.Red;
                        break;
                    default:
                        user.CardColor = defaultCardColor;
                        break;
                }
            }
            else if (e.Item is Event ev)
            {
                switch (e.Position)
                {
                    case DraggingCardPosition.OverThreshold:
                        ev.CardColor = e.Direction == SwipeCardDirection.Left
                            ? Colors.Green
                            : Colors.Red;
                        break;
                    default:
                        ev.CardColor = defaultCardColor;
                        break;
                }
            }
        }
        protected override async void OnAppearing()
        {
            await Favourites.LoadRejectedFromStorage();
            base.OnAppearing();

            // 1) Определяем своего пользователя
            var myUsername = AuthService.GetUsernameAsync();

            // 2) Обновляем локальный список взаимных лайков
            await Models.Favourites.RefreshFavoritesAsync();
            var mutualUsers = await AuthService.GetMutualUsersAsync(myUsername);
            Models.Favourites.mutual = new ObservableCollection<User>(mutualUsers);

            // 3) Инициализируем фильтры из сохранённых настроек
            Settings.Init(myUsername);

            // 4) Загружаем карточки и сразу применяем фильтрацию
            await FillEventsCardsAsync();
            await FilterCards();
        }



        //public async void FillUserCards()
        //{ 
        //    IsLoading = true;
        //    var username = AuthService.GetUsernameAsync();
        //    Settings.Init(username);

        //    await FillEventsCardsAsync();
        //    await FilterCards();

        //    IsLoading = false;
        //    RaiseEmptyFlags();
        //}

        public async Task FillUserCards()
        {
            // 1) Очищаем текущие результаты
            Users.Clear();

            // 2) Сбрасываем словарь конвертера перед новой загрузкой
            TagColorConverter.TagToCategory.Clear();

            try
            {
                var currentUsername = AuthService.GetUsernameAsync();
                var allUsers = await AuthService.GetAllUsersAsync();

                if (allUsers == null || !allUsers.Any())
                {
                    Debug.WriteLine("No users found in database");
                    return;
                }

                // 3) Фильтруем: исключаем себя и неполные профили
                var filteredProfiles = allUsers
                    .Where(u =>
                        u.username != currentUsername &&
        !Favourites.favorites.Any(f => f.Username == u.username) &&
        !Favourites.RejectedUsers.Contains(u.username) &&
        !Favourites.mutual.Any(m => m.Username == u.username) &&
                        !string.IsNullOrWhiteSpace(u.description) &&
                        !string.IsNullOrWhiteSpace(u.username) &&
                        !string.IsNullOrWhiteSpace(u.surname) &&
                        !string.IsNullOrWhiteSpace(u.vocation) &&
                        u.categories != null && u.categories.Count >= 1)
                    .ToList();

                // 4) Если выбраны категории — применяем дополнительный фильтр
                if (Settings.selectedUserCategories.Any())
                {
                    filteredProfiles = FilterUsersByCategories(filteredProfiles, Settings.selectedUserCategories.ToList());
                }

                // 5) Проходим по каждому профилю, наполняем конвертер и создаём User-модель
                foreach (var profile in filteredProfiles)
                {
                    // --- Наполняем словарь Tag -> Category для конвертера ---
                    if (profile.tags != null)
                    {
                        foreach (var kv in profile.tags)
                        {
                            string category = kv.Key;
                            foreach (var rawTag in kv.Value
                                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(t => t.Trim())
                                                      .Where(t => !string.IsNullOrEmpty(t)))
                            {
                                TagColorConverter.TagToCategory[rawTag] = category;
                            }
                        }
                    }
                    // ---------------------------------------------------------

                    // 6) Создаём и добавляем в коллекцию готовый User-объект
                    Users.Add(await CreateUserModelAsync(profile));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading users: {ex.Message}");
            }
            finally
            {
                RaiseEmptyFlags();
            }
        }

        private async Task<User> CreateUserModelAsync(AuthService.UserProfile profile)
        {
            var selectedCategories = Settings.selectedUserCategories.ToList();
            bool noCategorySelected = selectedCategories.Count == 0;

            // Фильтрация тегов
            var filteredTags = new List<string>();
            if (profile.tags != null)
            {
                if (noCategorySelected)
                {
                    foreach (var tagList in profile.tags.Values)
                    {
                        filteredTags.AddRange(tagList.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)));
                    }
                }
                else
                {
                    foreach (var category in selectedCategories)
                    {
                        if (profile.tags.ContainsKey(category))
                        {
                            filteredTags.AddRange(profile.tags[category].Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)));
                        }
                    }
                }
            }

            // Фильтрация навыков
            var filteredSkills = new List<string>();
            if (profile.skills != null)
            {
                if (noCategorySelected)
                {
                    foreach (var skillList in profile.skills.Values)
                    {
                        filteredSkills.AddRange(skillList.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
                    }
                }
                else
                {
                    foreach (var category in selectedCategories)
                    {
                        if (profile.skills.ContainsKey(category))
                        {
                            filteredSkills.AddRange(profile.skills[category].Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
                        }
                    }
                }
            }

            var user = new User(
                profile.username,
                $"{profile.name ?? ""} {profile.surname ?? ""}".Trim(),
                "",
                filteredTags,
                profile.vocation ?? "Не указано",
                profile.description ?? "Нет описания",
                string.Join(", ", filteredSkills),
                new List<string>()
            );

            var avatarImage = await AuthService.GetUserAvatarAsync(profile.username);
            user.Avatar = avatarImage ?? ImageSource.FromFile("default_profile_icon.png");

            return user;
        }



        private List<AuthService.UserProfile> FilterUsersByCategories( List<AuthService.UserProfile> users, List<string> selectedCategories)
        {
            return users.Where(user =>
            {
                // Если у пользователя нет категорий - пропускаем
                if (user.categories == null || !user.categories.Any())
                    return false;

                // Проверяем пересечение категорий пользователя с выбранными
                return user.categories.Intersect(selectedCategories).Any();
            }).ToList();
        }

        public async Task FillEventsCardsAsync()
        {
            _allEventsCache = new List<Event>();
            var allEventsData = await AuthService.GetAllEventsAsync();
            if (allEventsData == null) return;

            if (_allEventsCache == null)
                _allEventsCache = new List<Event>();

            var currentUser = AuthService.GetUsernameAsync();

            foreach (var ed in allEventsData)
            {
                // 1) Никогда не показываем СВОИ ивенты

                if (Favourites.favoriteEvents.Any(e => e.Hash == ed.hash) ||
    Favourites.RejectedEvents.Contains(ed.hash))
                {
                    continue;
                }

                if (ed.owner == currentUser)
                    continue;

                // 2) Приватные ивенты (public == 0) показываем ТОЛЬКО взаимным лайкам
                if (ed.@public == 0 &&
                    !Models.Favourites.mutual.Any(u => u.Username == ed.owner))
                {
                    continue;
                }

                // 3) Скипаем уже загруженные
                if (_allEventsCache.Any(e => e.Hash == ed.hash))
                    continue;

                // 4) Парсим и добавляем
                var start = DateTime.ParseExact(ed.date_from, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var end = DateTime.ParseExact(ed.date_to, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                _allEventsCache.Add(new Event(
                    ed.hash,
                    ed.name,
                    ed.categories ?? new List<string>(),
                    ed.description,
                    ed.location,
                    ed.@public == 1,
                    ed.online == 1)
                {
                    StartDate = start,
                    EndDate = end
                });
            }

            Events.Clear();
            foreach (var ev in _allEventsCache)
                Events.Add(ev);

            RaiseEmptyFlags();
        }

        public async Task FilterCards()
        {
            if (ProfilesEventsButtonStatus == 0)
            {
                await FillUserCards();
            }
            else {
                await FillEventsCardsAsync();

                // 1) Фильтр по public/online только если они заданы
                var list = Events.AsEnumerable();
                if (Settings.SelectedEventIsPublic.HasValue)
                    list = list.Where(ev => ev.IsPublic == Settings.SelectedEventIsPublic.Value);
                if (Settings.SelectedEventIsOnline.HasValue)
                    list = list.Where(ev => ev.IsOnline == Settings.SelectedEventIsOnline.Value);

                // 2) Категории
                var selCats = Settings.selectedEventCategories;
                IEnumerable<Event> exactCats, partialCats;
                if (selCats.Count == 0)
                {
                    exactCats = list;
                    partialCats = Enumerable.Empty<Event>();
                }
                else
                {
                    // точные по набору и дополнительные
                    exactCats = list.Where(ev => ev.Categories.Count == selCats.Count && selCats.All(c => ev.Categories.Contains(c)));
                    partialCats = list.Where(ev => ev.Categories.Any(c => selCats.Contains(c)) && !(ev.Categories.Count == selCats.Count && selCats.All(c => ev.Categories.Contains(c))));
                }

                // 3) Диапазон дат
                var sOpt = Settings.SelectedEventStartDate;
                var eOpt = Settings.SelectedEventEndDate;
                List<Event> final;
                if (!sOpt.HasValue || !eOpt.HasValue)
                {
                    final = exactCats.Concat(partialCats).ToList();
                }
                else
                {
                    var s = sOpt.Value.Date;
                    var e = eOpt.Value.Date;

                    // точное совпадение
                    var dateExact = exactCats.Where(ev => ev.StartDate.Date == s && ev.EndDate.Date == e);

                    // полностью внутри диапазона
                    var dateInside = exactCats.Concat(partialCats).Where(ev => ev.StartDate.Date >= s && ev.EndDate.Date <= e && !(ev.StartDate.Date == s && ev.EndDate.Date == e));

                    // пересекающиеся (начинаются до конца диапазона и заканчиваются после начала)
                    var dateOverlap = exactCats.Concat(partialCats).Where(ev => ev.StartDate.Date <= e&& ev.EndDate.Date >= s && !(ev.StartDate.Date >= s && ev.EndDate.Date <= e));

                    final = dateExact.Concat(dateInside).Concat(dateOverlap).ToList();
                }
                Events.Clear();
                foreach (var ev in final)
                    Events.Add(ev);
            }

            RaiseEmptyFlags();
        }


        private async void OnCardSwiped(SwipedCardEventArgs e)
        {
            if (e.Item is User swipedUser)
            {
                if (e.Direction == SwipeCardDirection.Left)
                {
                    Debug.WriteLine($"Liked: {swipedUser.Name}");

                    // Сохраняем в базу данных
                    string my_username = AuthService.GetUsernameAsync();

                    // Для моментального отображения
                    bool isAdded = false;

                    if (!Favourites.favorites.Any(u => u.Username == swipedUser.Username))
                    {
                        Favourites.favorites.Add(swipedUser);
                        Console.WriteLine("Добавлен ui");
                        isAdded = true;
                    }

                    bool success = await AuthService.AddSwipedUser(my_username, swipedUser.Username);
                    if (!success)
                    {
                        if (isAdded) Favourites.favorites.Remove(swipedUser);

                        Application.Current.MainPage.DisplayAlert("Try again, please", "Failed to save swiped user", "OK");
                    }

                }
                else if (e.Direction == SwipeCardDirection.Right)
                {
                    Favourites.RejectedUsers.Add(swipedUser.Username);
                    await Favourites.SaveRejectedToStorage();
                }
            }
            else if (e.Item is Event swipedEvent)
            {
                if (e.Direction == SwipeCardDirection.Left)
                {
                    Debug.WriteLine($"Liked event: {swipedEvent.Name}");

                    var username = AuthService.GetUsernameAsync();

                    bool savedSwipe = await AuthService.AddSwipedEvent(username, swipedEvent.Hash);
                    if (!savedSwipe)
                        Debug.WriteLine("Failed to save swiped event to server");

                    bool addedMember = await AuthService.AddEventMemberAsync(username, swipedEvent.Hash);
                    if (!addedMember)
                        Debug.WriteLine("Failed to add event member on server");

                    if (savedSwipe)
                    {
                        if (!Favourites.favoriteEvents.Any(ev => ev.Hash == swipedEvent.Hash))
                        {
                            Favourites.favoriteEvents.Add(swipedEvent);
                            Debug.WriteLine($"Event {swipedEvent.Name} added to favorites");
                        }
                    }
                }
                else if (e.Direction == SwipeCardDirection.Right)
                {
                    Favourites.RejectedEvents.Add(swipedEvent.Hash);
                    await Favourites.SaveRejectedToStorage();
                }
                Events.Remove(swipedEvent);
                _allEventsCache.RemoveAll(ev => ev.Hash == swipedEvent.Hash);
                await FillEventsCardsAsync();
                // await FilterCards(); 
            }
        }


        private void OnButtonSettingsClicked(object sender, System.EventArgs e)
        {
            this.ShowPopupAsync(new MainPageSettings(this));
        }

        public void OnButtonLupaClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Info", "you have clicked on the search button", "OK");
        }

        private bool theme = true;
        public void OnImageButtonClicked(object sender, EventArgs e)
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();
                if (theme)
                {
                    mergedDictionaries.Add(new LightTheme());
                    theme = false;
                }
                else
                {
                    mergedDictionaries.Add(new DarkTheme());
                    theme = true;
                }

                // Принудительно обновляем цвета кнопок
                UpdateButtonColors();

                // Обновляем цвета карточек
                Device.BeginInvokeOnMainThread(async () =>
                {
                    foreach (var user in Users)
                    {
                        user.UpdateCardColor();
                    }
                    foreach (var ev in Events)
                    {
                        ev.UpdateCardColor();
                    }
                });
            }
        } 

        private void UpdateButtonColors()
        {
            // Обновляем цвета на основе текущего состояния
            if (ProfilesEventsButtonStatus == 1)
            {
                EventsButton.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                ProfilesButton.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];

            }
            else
            {
                ProfilesButton.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                EventsButton.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];
            }
        }


        public async void OnEventsButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus == 1) return;
            ProfilesEventsButtonStatus = 1;

            UpdateButtonColors();
            swipeCardView.IsVisible = false;
            swipeCardViewEvent.IsVisible = true;

            await FilterCards();
        }


        public async void OnProfilesButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus == 0) return;
            ProfilesEventsButtonStatus = 0;

            UpdateButtonColors();
            swipeCardView.IsVisible = true;
            swipeCardViewEvent.IsVisible = false;

            await FillUserCards();
        }


    }
}
