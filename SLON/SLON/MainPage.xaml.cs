using CommunityToolkit.Maui.Views;
using Plugin.Maui.SwipeCardView.Core;
using SLON.Models;
using SLON.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SLON
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Event> Events { get; set; } = new();
        public ICommand OnCardSwipedCommand { get; }
        // 0 - профили, 1 - ивенты
        public int ProfilesEventsButtonStatus = 0;

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
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            FillUserCards();

            Events.Clear();
            FillEventsCards();
        }

        public async void FillUserCards()
        {
            Users.Clear();

            // Добавляем тестовые карточки пользователей
            //Users.Add(new User("alice", "Алиса Джигурда", new List<string> { "Art", "Design", "Innovation" }, "UI Designer", "Specialist in mobile app design", "Adobe XD, Figma, Photoshop"));
            //Users.Add(new User("bob", "Bob Smith", new List<string> { "Management", "Programming" }, "Backend Developer", "Focused on high-performance APIs", "C#, .NET, SQL"));
            //Users.Add(new User("carla", "Carla Perez", new List<string> { "Programming", "Cybersecurity" }, "Frontend Developer", "React expert with a focus on responsive design", "JavaScript, React, CSS"));
            //Users.Add(new User("david", "David Lee", new List<string> { "Programming", "Cybersecurity" }, "Data Scientist", "Experienced in AI and machine learning", "Python, TensorFlow, PyTorch"));
            //Users.Add(new User("emma", "Emma Brown", new List<string> { "Physics", "Cybersecurity" }, "Marketing Manager", "Specialist in social media campaigns", "SEO, Content Marketing, Google Ads"));
            //Users.Add(new User("frank", "Frank Wilson", new List<string> { "Marketing", "Networking" }, "DevOps Engineer", "Focus on CI/CD pipelines", "Docker, Kubernetes, Jenkins"));
            //Users.Add(new User("grace", "Grace Adams", new List<string> { "Management", "Marketing" }, "Project Manager", "Certified Scrum Master", "Agile, Scrum, Kanban"));
            //Users.Add(new User("henry", "Henry Carter", new List<string> { "Programming", "Biology" }, "Blockchain Developer", "Expert in smart contracts", "Solidity, Ethereum, Web3"));
            //Users.Add(new User("ivy", "Ivy Martinez", new List<string> { "Creativity", "Learning" }, "Content Writer", "Crafts engaging stories", "Copywriting, Blogging, SEO Writing"));
            //Users.Add(new User("jack", "Jack Robinson", new List<string> { "AI", "Physics" }, "Cybersecurity Specialist", "Focus on network security", "Penetration Testing, Firewalls, Ethical Hacking"));
            //Users.Add(new User("lara", "Lara Croft", new List<string> { "Fitness", "Football" }, "Adventurer", "Explorer and athlete", "Climbing, Parkour"));

            try
            {
                Users.Clear();

                var currentUsername = AuthService.GetUsernameAsync();

                // Получаем всех пользователей из базы
                var allUsers = await AuthService.GetAllUsersAsync();
                if (allUsers == null || !allUsers.Any())
                {
                    Debug.WriteLine("No users found in database");
                    return;
                }

                // Фильтруем: исключаем текущего пользователя и тех, кто уже был свайпнут
                var filteredUsers = allUsers
                    .Where(u => (u.username != currentUsername) 
                                && (u.description != "") 
                                && (u.username != "")
                                && (u.surname != "")
                                && (u.vocation != "")
                                && (u.categories.Count >= 1))
                    .ToList();

                // Применяем фильтр по категориям, если они выбраны
                if (Settings.selectedUserCategories.Any())
                {
                    var selectedCategoriesList = Settings.selectedUserCategories.ToList();
                    filteredUsers = FilterUsersByCategories(filteredUsers, selectedCategoriesList);
                }

                // Добавляем в коллекцию
                foreach (var user in filteredUsers)
                {
                    Users.Add(CreateUserModel(user));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        private User CreateUserModel(AuthService.UserProfile profile)
        {
            var selectedCategories = Settings.selectedUserCategories.ToList();
            bool noCategorySelected = selectedCategories.Count == 0; // Проверяем, выбраны ли категории

            var filteredTags = new List<string>();
            if (profile.tags != null)
            {
                if (noCategorySelected)
                {
                    // Если категории не выбраны, добавляем все теги из всех категорий
                    foreach (var tagList in profile.tags.Values)
                    {
                        filteredTags.AddRange(tagList.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)));
                    }
                }
                else
                {
                    // Если категории выбраны, добавляем только теги из этих категорий
                    foreach (var category in selectedCategories)
                    {
                        if (profile.tags.ContainsKey(category))
                        {
                            filteredTags.AddRange(profile.tags[category].Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)));
                        }
                    }
                }
            }

            var filteredSkills = new List<string>();
            if (profile.skills != null)
            {
                if (noCategorySelected)
                {
                    // Если категории не выбраны, добавляем все навыки из всех категорий
                    foreach (var skillList in profile.skills.Values)
                    {
                        filteredSkills.AddRange(skillList.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
                    }
                }
                else
                {
                    // Если категории выбраны, добавляем только навыки из этих категорий
                    foreach (var category in selectedCategories)
                    {
                        if (profile.skills.ContainsKey(category))
                        {
                            filteredSkills.AddRange(profile.skills[category].Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
                        }
                    }
                }
            }

            return new User(
                username: profile.username,
                name: $"{profile.name ?? ""} {profile.surname ?? ""}".Trim(),
                tags: filteredTags,
                vocation: profile.vocation ?? "Не указано",
                info: profile.description ?? "Нет описания",
                skills: string.Join(", ", filteredSkills)
            )
            {
                Username = profile.username
            };
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

        public void FillEventsCards()
        {
            // Тестовые карточки событий с заполненными датами
            Events.Add(new Event(1, "Tech Conference 2024", new List<string> { "Creation", "Education", "IT" },
                "A conference on the latest trends in mobile app design, APIs, and UI/UX",
                "Ulitsa 2-ya Krivorozhskaya, Rostov-on-Don", false, false)
            {
                StartDate = new DateTime(2024, 5, 1),
                EndDate = new DateTime(2024, 5, 3)
            });
            Events.Add(new Event(2, "Backend Development Workshop", new List<string> { "IT", "Science", "Business" },
                "A hands-on workshop focusing on building high-performance APIs",
                "Ulitsa Budennovskiy, Rostov-on-Don", false, false)
            {
                StartDate = new DateTime(2024, 6, 10),
                EndDate = new DateTime(2024, 6, 10)
            });
            Events.Add(new Event(3, "React Masterclass", new List<string> { "Education", "Creation" },
                "An in-depth session on mastering React and responsive design",
                "Ulitsa Berzhaninova, Rostov-on-Don", true, true)
            {
                StartDate = new DateTime(2024, 7, 5),
                EndDate = new DateTime(2024, 7, 5)
            });
            Events.Add(new Event(4, "AI and Machine Learning Summit", new List<string> { "Science", "IT", "Business" },
                "A summit dedicated to AI advancements and machine learning applications",
                "Ulitsa Chekistov, Rostov-on-Don", false, false)
            {
                StartDate = new DateTime(2024, 8, 15),
                EndDate = new DateTime(2024, 8, 17)
            });
            Events.Add(new Event(5, "Social Media Marketing Bootcamp", new List<string> { "IT", "Education", "Social" },
                "A bootcamp focused on building effective social media campaigns",
                "Ulitsa Pushkinskaya, Rostov-on-Don", false, false)
            {
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 9, 1)
            });
            Events.Add(new Event(6, "DevOps and Automation Training", new List<string> { "IT", "Science", "Education" },
                "A training session on CI/CD pipelines and DevOps tools",
                "Ulitsa Sovetskaya, Rostov-on-Don", true, true)
            {
                StartDate = new DateTime(2024, 10, 10),
                EndDate = new DateTime(2024, 10, 11)
            });
            Events.Add(new Event(7, "Agile Project Management Seminar", new List<string> { "Business", "Education", "IT" },
                "A seminar on Agile methodologies, Scrum, and Kanban",
                "Ulitsa Lenina, Rostov-on-Don", false, false)
            {
                StartDate = new DateTime(2024, 11, 5),
                EndDate = new DateTime(2024, 11, 5)
            });
            Events.Add(new Event(8, "Blockchain & Crypto Conference", new List<string> { "Science", "IT", "Business" },
                "A conference focused on the latest in blockchain technology and smart contracts",
                "Ulitsa Maksima Gorkogo, Rostov-on-Don", true, true)
            {
                StartDate = new DateTime(2024, 12, 1),
                EndDate = new DateTime(2024, 12, 2)
            });
            Events.Add(new Event(9, "Content Writing for SEO", new List<string> { "Business", "Creation", "Education" },
                "A workshop on creating SEO-friendly content for blogs and websites",
                "Ulitsa Nekrasova, Rostov-on-Don", true, true)
            {
                StartDate = new DateTime(2024, 4, 20),
                EndDate = new DateTime(2024, 4, 20)
            });
            Events.Add(new Event(10, "Cybersecurity Awareness Workshop", new List<string> { "Science", "IT", "Education" },
                "A workshop on network security and ethical hacking",
                "Ulitsa Molodezhnyy, Rostov-on-Don", true, true)
            {
                StartDate = new DateTime(2024, 3, 15),
                EndDate = new DateTime(2024, 3, 15)
            });
            Events.Add(new Event(11, "Health & Wellness Expo", new List<string> { "Health", "Social" },
                "An expo dedicated to wellness, fitness, and nutrition",
                "Ulitsa Zdorovya, Rostov-on-Don", true, false)
            {
                StartDate = new DateTime(2024, 5, 20),
                EndDate = new DateTime(2024, 5, 20)
            });
            Events.Add(new Event(12, "Health & Wellness Expo", new List<string> { "Health", "Social" },
                "An expo dedicated to wellness, fitness, and nutrition",
                "Ulitsa Zdorovya, Rostov-on-Don", true, false)
            {
                StartDate = new DateTime(2024, 5, 21),
                EndDate = new DateTime(2024, 5, 21)
            });
            Events.Add(new Event(13, "Health & Wellness Expo", new List<string> { "Health", "Social" },
                "An expo dedicated to wellness, fitness, and nutrition",
                "Ulitsa Zdorovya, Rostov-on-Don", true, false)
            {
                StartDate = new DateTime(2024, 5, 22),
                EndDate = new DateTime(2024, 5, 22)
            });
            Events.Add(new Event(14, "Health & Wellness Expo", new List<string> { "Health", "Social" },
                    "An expo dedicated to wellness, fitness, and nutrition",
                    "Ulitsa Zdorovya, Rostov-on-Don", true, false)
            {
                StartDate = new DateTime(2024, 5, 23),
                EndDate = new DateTime(2024, 5, 23)
            });
        }

        public void FilterCards()
        {
            if (ProfilesEventsButtonStatus == 0)
            {
                Users.Clear();
                FillUserCards();

                if (Settings.selectedUserCategories.Count > 0)
                {
                    List<User> filteredUsers = new List<User>();

                    // Если выбрана только одна категория:
                    if (Settings.selectedUserCategories.Count == 1)
                    {
                        var selectedCategory = Settings.selectedUserCategories.First();
                        if (!TagCategories.Categories.ContainsKey(selectedCategory))
                        {
                            Debug.WriteLine($"Не найдены теги для категории: {selectedCategory}");
                            return;
                        }
                        var allowedTags = TagCategories.Categories[selectedCategory];

                        // Фильтруем только тех пользователей, у которых все теги принадлежат выбранной категории
                        filteredUsers = Users.Where(user =>
                            user.Tags.Any() && user.Tags.All(tag => allowedTags.Contains(tag))
                        ).ToList();
                    }
                    else // Если выбрано 2 и более категорий
                    {
                        // Собираем объединённый набор разрешённых тегов из выбранных категорий
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

                        // Группа 1: пользователи, у которых все теги входят в разрешённый набор
                        var group1 = Users.Where(user =>
                            user.Tags.Any() && user.Tags.All(tag => allowedTags.Contains(tag))
                        ).ToList();

                        // Группа 2: пользователи, у которых есть хотя бы один разрешённый тег, но не все теги из него
                        var group2 = Users.Where(user =>
                            user.Tags.Any(tag => allowedTags.Contains(tag)) &&
                            user.Tags.Any(tag => !allowedTags.Contains(tag))
                        ).ToList();

                        filteredUsers = group1.Concat(group2).ToList();
                    }

                    // Очищаем исходную коллекцию и добавляем отфильтрованных пользователей
                    Users.Clear();
                    foreach (var user in filteredUsers)
                    {
                        Users.Add(user);
                        Debug.WriteLine($"Добавлен пользователь: {user.Name}");
                    }
                }
            }
            else if (ProfilesEventsButtonStatus == 1) // Фильтрация событий (по категориям)
            {
                Events.Clear();
                FillEventsCards();
                var filteredEvents = Events.Where(ev =>
                    (Settings.selectedEventCategories.Count == 0 ||
                     ev.Categories.Any(tag => Settings.selectedEventCategories.Contains(tag)))
                ).ToList();

                Events.Clear();
                foreach (var ev in filteredEvents)
                {
                    Events.Add(ev);
                    Debug.WriteLine($"Добавлено событие: {ev.Name}");
                }
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
                        
                    bool success = await AuthService.AddSwipedUser(my_username, swipedUser.Username);
                    if (!success)
                    {
                        Debug.WriteLine("Failed to save swiped user to server");
                    }

                    if (!Favourites.favorites.Any(u => u.Id == swipedUser.Id))
                    {
                        swipedUser.IsILikedHim = true;
                        Favourites.favorites.Add(swipedUser);

                        // здесь проверка взаимного лайка
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
                    if (!Favourites.favoriteEvents.Any(ev => ev.Id == swipedEvent.Id))
                    {
                        Favourites.favoriteEvents.Add(swipedEvent);
                        Debug.WriteLine($"Event {swipedEvent.Name} added to favorites");
                    }
                }
                else if (e.Direction == SwipeCardDirection.Right)
                {
                    Debug.WriteLine($"Skipped event: {swipedEvent.Name}");
                }
            }
        }

        private void OnButtonSettingsClicked(object sender, System.EventArgs e)
        {
            this.ShowPopupAsync(new MainPageSettings(this));
        }

        public void OnEventsButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus == 1) return;
            ProfilesEventsButtonStatus = 1;
            EventsButton.BackgroundColor = Color.FromArgb("#915AC5");
            ProfilesButton.BackgroundColor = Color.FromArgb("#292929");
            swipeCardView.IsVisible = false;
            swipeCardViewEvent.IsVisible = true;
        }

        public void OnProfilesButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus == 0) return;
            ProfilesEventsButtonStatus = 0;
            ProfilesButton.BackgroundColor = Color.FromArgb("#915AC5");
            EventsButton.BackgroundColor = Color.FromArgb("#292929");
            swipeCardView.IsVisible = true;
            swipeCardViewEvent.IsVisible = false;
        }

        public void OnButtonLupaClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Info", "you have clicked on the search button", "OK");
        }
    }
}
