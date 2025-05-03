using CommunityToolkit.Maui.Views;
using Plugin.Maui.SwipeCardView.Core;
using SLON.Models;
using SLON.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Globalization;

namespace SLON
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Event> Events { get; set; } = new();
        public ICommand OnCardSwipedCommand { get; }
        // 0 - профили, 1 - ивенты
        public int ProfilesEventsButtonStatus = 0;
        private List<Event> _allEventsCache;

        public MainPage()
        {
            InitializeComponent();

            OnCardSwipedCommand = new Command<SwipedCardEventArgs>(OnCardSwiped);
            BindingContext = this;

            Shell.SetNavBarIsVisible(this, false);

            swipeCardView.Dragging += OnDragging;
            swipeCardViewEvent.Dragging += OnDragging;
        }

        private void OnDragging(object sender, DraggingCardEventArgs e)
        {
            if (e.Item is User user)
            {
                switch (e.Position)
                {
                    case DraggingCardPosition.OverThreshold:
                        if (e.Direction == SwipeCardDirection.Left)
                            user.CardColor = Colors.Green;
                        else if (e.Direction == SwipeCardDirection.Right)
                            user.CardColor = Colors.Red;
                        break;
                    default:
                        user.CardColor = Color.FromArgb("#292929");
                        break;
                }
            }
            else if (e.Item is Event ev)
            {
                switch (e.Position)
                {
                    case DraggingCardPosition.OverThreshold:
                        if (e.Direction == SwipeCardDirection.Left)
                            ev.CardColor = Colors.Green;
                        else if (e.Direction == SwipeCardDirection.Right)
                            ev.CardColor = Colors.Red;
                        break;
                    default:
                        ev.CardColor = Color.FromArgb("#292929");
                        break;
                }
            }
        }
        protected override async void OnAppearing()
        {
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



        public async void FillUserCards()
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
                string.Join(", ", filteredSkills)
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
            var allEventsData = await AuthService.GetAllEventsAsync();
            if (allEventsData == null)
                return;

            if (_allEventsCache == null)
                _allEventsCache = new List<Event>();

            var currentUser = AuthService.GetUsernameAsync();

            foreach (var ed in allEventsData)
            {
                // 1) Никогда не показываем СВОИ ивенты
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

            // 5) Обновляем ObservableCollection для UI
            Events.Clear();
            foreach (var ev in _allEventsCache)
                Events.Add(ev);
        }



        public async Task FilterCards()
        {
            if (ProfilesEventsButtonStatus == 0)
            {
                Users.Clear();
                FillUserCards();

                if (Settings.selectedUserCategories.Count > 0)
                {
                    List<User> filteredUsers = new List<User>();

                    if (Settings.selectedUserCategories.Count == 1)
                    {
                        var selectedCategory = Settings.selectedUserCategories.First();
                        if (!TagCategories.Categories.ContainsKey(selectedCategory))
                        {
                            Debug.WriteLine($"Не найдены теги для категории: {selectedCategory}");
                            return;
                        }
                        var allowedTags = TagCategories.Categories[selectedCategory];

                        filteredUsers = Users.Where(user =>
                            user.Tags.Any() && user.Tags.All(tag => allowedTags.Contains(tag))
                        ).ToList();
                    }
                    else
                    {
                        var allowedTags = new HashSet<string>();
                        foreach (var category in Settings.selectedUserCategories)
                        {
                            if (TagCategories.Categories.ContainsKey(category))
                            {
                                foreach (var tag in TagCategories.Categories[category])
                                {
                                    allowedTags.Add(tag);
                                }
                            }
                        }

                        var group1 = Users.Where(user =>
                            user.Tags.Any() && user.Tags.All(tag => allowedTags.Contains(tag))
                        ).ToList();

                        var group2 = Users.Where(user =>
                            user.Tags.Any(tag => allowedTags.Contains(tag)) &&
                            user.Tags.Any(tag => !allowedTags.Contains(tag))
                        ).ToList();

                        filteredUsers = group1.Concat(group2).ToList();
                    }

                    Users.Clear();
                    foreach (var user in filteredUsers)
                    {
                        Users.Add(user);
                        Debug.WriteLine($"Добавлен пользователь: {user.Name}");
                    }
                }
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
                    Debug.WriteLine($"Skipped: {swipedUser.Name}");
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
            }
        }


        private void OnButtonSettingsClicked(object sender, System.EventArgs e)
        {
            this.ShowPopupAsync(new MainPageSettings(this));
        }

        public async void OnEventsButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus == 1) return;
            ProfilesEventsButtonStatus = 1;
            EventsButton.BackgroundColor = Color.FromArgb("#915AC5");
            ProfilesButton.BackgroundColor = Color.FromArgb("#292929");
            swipeCardView.IsVisible = false;
            swipeCardViewEvent.IsVisible = true;

            await FilterCards();
        }


        public void OnProfilesButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus == 0) return;
            ProfilesEventsButtonStatus = 0;
            ProfilesButton.BackgroundColor = Color.FromArgb("#915AC5");
            EventsButton.BackgroundColor = Color.FromArgb("#292929");
            swipeCardView.IsVisible = true;
            swipeCardViewEvent.IsVisible = false;

            FillUserCards();
        }


        public void OnButtonLupaClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Info", "you have clicked on the search button", "OK");
        }
    }
}
