using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Layouts;
using CommunityToolkit.Maui.Views;
using Plugin.Maui.SwipeCardView;
using Plugin.Maui.SwipeCardView.Core;
using SLON.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace SLON
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<User> Users { get; set; } = new();

        public ObservableCollection<Event> Events { get; set; } = new();
        public ICommand OnCardSwipedCommand { get; }
        public ICommand OnCardSwipedEventCommand { get; }

        // profiles = 0; events = 1
        public int ProfilesEventsButtonStatus = 0;

        public MainPage()
        {
            InitializeComponent();
            OnCardSwipedCommand = new Command<SwipedCardEventArgs>(OnCardSwiped);

            OnCardSwipedEventCommand = new Command<SwipedCardEventArgs>(OnCardSwipedEvent);
            BindingContext = this;

        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            Users.Clear();
            Events.Clear();

            FillUserCards();
            FillEventsCards();
        }

        public void FillUserCards()
        {
<<<<<<< HEAD
            Users.Add(new User(1, "Алиса Джигурда", "IT, Creation", "UI Designer", "Specialist in mobile app design", "Adobe XD, Figma, Photoshop"));
            Users.Add(new User(2, "Bob Smith", "IT", "Backend Developer", "Focused on high-performance APIs", "C#, .NET, SQL"));
            Users.Add(new User(3, "Carla Perez", "IT, Creation", "Frontend Developer", "React expert with a focus on responsive design", "JavaScript, React, CSS"));
            Users.Add(new User(4, "David Lee", "IT, Science", "Data Scientist", "Experienced in AI and machine learning", "Python, TensorFlow, PyTorch"));
            Users.Add(new User(5, "Emma Brown", "Business, Creation", "Marketing Manager", "Specialist in social media campaigns", "SEO, Content Marketing, Google Ads"));
            Users.Add(new User(6, "Frank Wilson", "IT, Business", "DevOps Engineer", "Focus on CI/CD pipelines", "Docker, Kubernetes, Jenkins"));
            Users.Add(new User(7, "Grace Adams", "Business, Education", "Project Manager", "Certified Scrum Master", "Agile, Scrum, Kanban"));
            Users.Add(new User(8, "Henry Carter", "IT, Business", "Blockchain Developer", "Expert in smart contracts", "Solidity, Ethereum, Web3"));
            Users.Add(new User(9, "Ivy Martinez", "Creation, Education", "Content Writer", "Crafts engaging stories", "Copywriting, Blogging, SEO Writing"));
            Users.Add(new User(10, "Jack Robinson", "IT, Science", "Cybersecurity Specialist", "Focus on network security", "Penetration Testing, Firewalls, Ethical Hacking"));
=======
            Users.Add(new User(1, "Алиса Джигурда", new List<string> { "Art", "Design", "Innovation" }, "UI Designer", "Specialist in mobile app design", "Adobe XD, Figma, Photoshop"));
            Users.Add(new User(2, "Bob Smith", new List<string> { "Management", "Programming" }, "Backend Developer", "Focused on high-performance APIs", "C#, .NET, SQL"));
            Users.Add(new User(3, "Carla Perez", new List<string> { "Programming", "Cybersecurity" }, "Frontend Developer", "React expert with a focus on responsive design", "JavaScript, React, CSS"));
            Users.Add(new User(4, "David Lee", new List<string> { "Programming", "Cybersecurity" }, "Data Scientist", "Experienced in AI and machine learning", "Python, TensorFlow, PyTorch"));
            Users.Add(new User(5, "Emma Brown", new List<string> { "Physics", "Cybersecurity" }, "Marketing Manager", "Specialist in social media campaigns", "SEO, Content Marketing, Google Ads"));
            Users.Add(new User(6, "Frank Wilson", new List<string> { "Marketing", "Networking" }, "DevOps Engineer", "Focus on CI/CD pipelines", "Docker, Kubernetes, Jenkins"));
            Users.Add(new User(7, "Grace Adams", new List<string> { "Management", "Marketing" }, "Project Manager", "Certified Scrum Master", "Agile, Scrum, Kanban"));
            Users.Add(new User(8, "Henry Carter", new List<string> { "IT", "Biology" }, "Blockchain Developer", "Expert in smart contracts", "Solidity, Ethereum, Web3"));
            Users.Add(new User(9, "Ivy Martinez", new List<string> { "Creation", "Education" }, "Content Writer", "Crafts engaging stories", "Copywriting, Blogging, SEO Writing"));
            Users.Add(new User(10, "Jack Robinson", new List<string> { "AI", "Science" }, "Cybersecurity Specialist", "Focus on network security", "Penetration Testing, Firewalls, Ethical Hacking"));

            OnPropertyChanged(nameof(Users));
>>>>>>> eca5211 (Update Main_Page)

            var usersToAdd = Users.Where(user => !Favourites.favorites.Any(fav => fav.Id == user.Id)).ToList();
            foreach (var user in usersToAdd)
            {
                Users.Add(user);
            }
        }

        public void FillEventsCards()
        {
<<<<<<< HEAD
            Events.Add(new Event(1, "Tech Conference 2024", "IT, Education", "A conference on the latest trends in mobile app design, APIs, and UI/UX", "Ulitsa 2-ya Krivorozhskaya, Rostov-on-Don"));
            Events.Add(new Event(2, "Backend Development Workshop", "IT, Education", "A hands-on workshop focusing on building high-performance APIs", "Ulitsa Budennovskiy, Rostov-on-Don"));
            Events.Add(new Event(3, "React Masterclass", "IT, Education", "An in-depth session on mastering React and responsive design", "Ulitsa Berzhaninova, Rostov-on-Don"));
            Events.Add(new Event(4, "AI and Machine Learning Summit", "IT, Science", "A summit dedicated to AI advancements and machine learning applications", "Ulitsa Chekistov, Rostov-on-Don"));
            Events.Add(new Event(5, "Social Media Marketing Bootcamp", "Business, Education", "A bootcamp focused on building effective social media campaigns", "Ulitsa Pushkinskaya, Rostov-on-Don"));
            Events.Add(new Event(6, "DevOps and Automation Training", "IT, Education", "A training session on CI/CD pipelines and DevOps tools", "Ulitsa Sovetskaya, Rostov-on-Don"));
            Events.Add(new Event(7, "Agile Project Management Seminar", "Business, Education", "A seminar on Agile methodologies, Scrum, and Kanban", "Ulitsa Lenina, Rostov-on-Don"));
            Events.Add(new Event(8, "Blockchain & Crypto Conference", "IT, Business", "A conference focused on the latest in blockchain technology and smart contracts", "Ulitsa Maksima Gorkogo, Rostov-on-Don"));
            Events.Add(new Event(9, "Content Writing for SEO", "Creation, Business", "A workshop on creating SEO-friendly content for blogs and websites", "Ulitsa Nekrasova, Rostov-on-Don"));
            Events.Add(new Event(10, "Cybersecurity Awareness Workshop", "IT, Education", "A workshop on network security and ethical hacking", "Ulitsa Molodezhnyy, Rostov-on-Don"));

=======
            Events.Add(new Event(1, "Tech Conference 2024", new List<string> { "IT", "Education" }, "A conference on the latest trends", "Rostov-on-Don", true, true));
            Events.Add(new Event(2, "Conference 2025", new List<string> { "IT" }, "A conference on the latest trends", "Rostov-on-Don", false, false));
            Events.Add(new Event(3, "React Masterclass", new List<string> { "Education", "Creation" }, "An in-depth session on mastering React and responsive design", "Ulitsa Berzhaninova, Rostov-on-Don", true, true));
            Events.Add(new Event(4, "AI and Machine Learning Summit", new List<string> { "Science", "IT", "Business" }, "A summit dedicated to AI advancements and machine learning applications", "Ulitsa Chekistov, Rostov-on-Don", false, false));
            Events.Add(new Event(5, "Social Media Marketing Bootcamp", new List<string> { "IT", "Education" }, "A bootcamp focused on building effective social media campaigns", "Ulitsa Pushkinskaya, Rostov-on-Don", true, true));
            Events.Add(new Event(6, "DevOps and Automation Training", new List<string> { "IT", "Science", "Education" }, "A training session on CI/CD pipelines and DevOps tools", "Ulitsa Sovetskaya, Rostov-on-Don", false, false));
            Events.Add(new Event(7, "Agile Project Management Seminar", new List<string> { "Business", "Education", "IT" }, "A seminar on Agile methodologies, Scrum, and Kanban", "Ulitsa Lenina, Rostov-on-Don", true, true));
            Events.Add(new Event(8, "Blockchain & Crypto Conference", new List<string> { "Science", "IT", "Business" }, "A conference focused on the latest in blockchain technology and smart contracts", "Ulitsa Maksima Gorkogo, Rostov-on-Don", false, false));
            Events.Add(new Event(9, "Content Writing for SEO", new List<string> { "Business", "Creation", "Education" }, "A workshop on creating SEO-friendly content for blogs and websites", "Ulitsa Nekrasova, Rostov-on-Don", false, false));

            //OnPropertyChanged(nameof(Events));
>>>>>>> eca5211 (Update Main_Page)
        }


        private void ColorUserTags(User user)
        {
            foreach (var category in user.Tags)
            {
                // Выбираем цвет в зависимости от того, выбрана ли категория для профилей
                bool isSelected = Settings.selectedUserCategories.Contains(category);
                var bgColor = isSelected ? GetCategoryColor(category) : Color.FromRgb(129, 129, 129);
                var height = category.Length > 14 ? 70 : 10;

                var frame = new Frame
                {
                    BackgroundColor = bgColor,
                    BorderColor = Colors.LightGray,
                    CornerRadius = 20,
                    Padding = new Thickness(10, 5),
                    Margin = new Thickness(5),
                    HeightRequest = height,
                    Content = new Label
                    {
                        Text = category,
                        TextColor = Colors.White,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };

                // Добавьте frame в нужный контейнер (например, в FlexLayout или StackLayout)
            }
        }

        private void ColorEventTags(Event ev)
        {
            foreach (var category in ev.Categories)
            {
                bool isSelected = Settings.selectedEventCategories.Contains(category);
                var bgColor = isSelected ? GetCategoryColor(category) : Color.FromRgb(129, 129, 129);
                var height = category.Length > 14 ? 70 : 10;

                var frame = new Frame
                {
                    BackgroundColor = bgColor,
                    BorderColor = Colors.LightGray,
                    CornerRadius = 20,
                    Padding = new Thickness(10, 5),
                    Margin = new Thickness(5),
                    HeightRequest = height,
                    Content = new Label
                    {
                        Text = category,
                        TextColor = Colors.White,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };


            }
        }

        private async void OnCardSwiped(SwipedCardEventArgs e)
        {
            if (e.Item is User swipedUser)
            {
                // Плавное изменение цвета при начале свайпа
                if (e.Direction == SwipeCardDirection.Left)
                {
<<<<<<< HEAD
                    Debug.WriteLine($"Liked: {swipedUser.Name}");
                    Debug.WriteLine($"count: {Favourites.favorites.Count}");
=======
>>>>>>> eca5211 (Update Main_Page)

                    if (!Favourites.favorites.Any(fav => fav.Id == swipedUser.Id))
                    {
                        Favourites.favorites.Add(swipedUser);
                        Debug.WriteLine($"Added to favourites: {swipedUser.Name}");
                    }
                }
                else if (e.Direction == SwipeCardDirection.Right)
                {
                    /*to do*/


                }
            }
<<<<<<< HEAD
            else
            {
                Debug.WriteLine("Swiped item is not a User");
=======
        }

        private async void OnCardSwipedEvent(SwipedCardEventArgs e)
        {
            if (e.Item is Event swipedEvent)
            {
                // Плавное изменение цвета при начале свайпа
                if (e.Direction == SwipeCardDirection.Left)
                {

                    if (!Favourites.favorites.Any(fav => fav.Id == swipedEvent.Id))
                    {
                        //Favourites.favorites.Add(swipedEvent);
                        Debug.WriteLine($"Added to favourites: {swipedEvent.Name}");
                    }
                }
                else if (e.Direction == SwipeCardDirection.Right)
                {
                    /*do to*/
                }



>>>>>>> eca5211 (Update Main_Page)
            }
        }

        public void FilterCards()
        {
<<<<<<< HEAD
            Users.Clear();
            FillUserCards();

            if (Settings.selectedCategories.Count > 0)
            {
                ObservableCollection<User> filteredUsers = new();

                foreach (User user in Users)
                {
                    int flag = 0;

                    foreach (string category in Settings.selectedCategories)
                    {
                        if (user.Tags.Contains(category))
                        {
                            flag = 1;
                            break;
                        }
                    }

                    if (flag == 1)
                    {
                        filteredUsers.Add(user);
                        Debug.WriteLine($"Добавлен {user.Name}");
                    }
                }

                Users.Clear();
                foreach (User user in filteredUsers)
                {
                    Debug.WriteLine(user.Name);
                    Users.Add(user);
=======
            if (ProfilesEventsButtonStatus == 0) // Фильтрация профилей
            {
                Users.Clear();
                FillUserCards();

                if (Settings.selectedUserCategories.Count > 0)
                {
                    // Фильтруем пользователей:
                    // Если для хотя бы одной выбранной категории в глобальном словаре найден тег,
                    // который присутствует в списке тегов пользователя, то карточка проходит фильтр.
                    var filteredUsers = Users.Where(user =>
                        // Проверяем, что все теги пользователя принадлежат выбранным категориям
                        user.Tags.All(tag =>
                            Settings.selectedUserCategories.Any(category =>
                                TagCategories.Categories.ContainsKey(category) &&
                                TagCategories.Categories[category].Contains(tag)
                            )
                        )
                    ).ToList();

                    Users.Clear();
                    foreach (var user in filteredUsers)
                    {
                        Users.Add(user);
                        Debug.WriteLine($"Добавлен пользователь: {user.Name}");
                    }
                }
            }
            else if (ProfilesEventsButtonStatus == 1) // Фильтрация событий
            {
                Events.Clear();
                FillEventsCards();

                if (Settings.selectedEventCategories.Count > 0)
                {
                    var filteredEvents = Events.Where(ev =>
                        ev.Categories.All(tag => Settings.selectedEventCategories.Contains(tag))
                    ).ToList();

                    Events.Clear();
                    foreach (var ev in filteredEvents)
                    {
                        Events.Add(ev);
                        Debug.WriteLine($"Добавлено событие: {ev.Name}");
                    }
>>>>>>> eca5211 (Update Main_Page)
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

            EventsButton.BackgroundColor = Color.FromArgb("#915AC5"); // Фиолетовый
            ProfilesButton.BackgroundColor = Color.FromArgb("#292929"); // Серый

            swipeCardView.IsVisible = true;
        }

        public void OnProfilesButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus == 0) return;

            ProfilesEventsButtonStatus = 0;

            ProfilesButton.BackgroundColor = Color.FromArgb("#915AC5"); // Фиолетовый
            EventsButton.BackgroundColor = Color.FromArgb("#292929"); // Серый

            swipeCardView.IsVisible = true;
            //swipeCardViewEvent.IsVisible = false;
        }
    }
    public class TagsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tags && !string.IsNullOrWhiteSpace(tags))
            {
                return tags.Split(new char[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim())
                           .ToList();
            }
            return new List<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }


        public void OnButtonLupaClicked(object sender, EventArgs e)
        {

            Application.Current.MainPage.DisplayAlert("Info", "you have clicked on the search button", "OK");
        }


        private Color GetCategoryColor(string category)
        {
            return category switch
            {
                "Creation" => Color.FromArgb("#008f39"),
                "Education" => Color.FromArgb("#a020f0"),
                "IT" => Color.FromArgb("#00a6ff"),
                "Social" => Color.FromArgb("#d11f1f"),
                "Business" => Color.FromArgb("#d11f1f"),
                "Science" => Color.FromArgb("#d11f1f"),


                //Creation
                "Art" => Color.FromArgb("#008f39"),
                "Design" => Color.FromArgb("#008f39"),
                "Innovation" => Color.FromArgb("#008f39"),
                "Creativity" => Color.FromArgb("#008f39"),
                "Prototyping" => Color.FromArgb("#008f39"),

                //"Education"
                "Learning" => Color.FromArgb("#a020f0"),
                "Teaching" => Color.FromArgb("#a020f0"),
                "Courses" => Color.FromArgb("#a020f0"),
                "Workshops" => Color.FromArgb("#a020f0"),
                "Tutoring" => Color.FromArgb("#a020f0"),

                // IT
                "Programming" => Color.FromArgb("#00a6ff"),
                "Cybersecurity" => Color.FromArgb("#00a6ff"),
                "AI" => Color.FromArgb("#00a6ff"),
                "Cloud Computing" => Color.FromArgb("#00a6ff"),

                //Social
                "Community" => Color.FromArgb("#d11f1f"),
                "Networking" => Color.FromArgb("#d11f1f"),
                "Events" => Color.FromArgb("#d11f1f"),
                "Charity" => Color.FromArgb("#d11f1f"),
                "Social Media" => Color.FromArgb("#d11f1f"),

                //Business

                "Entrepreneurship" => Color.FromArgb("#d11f1f"),
                "Management" => Color.FromArgb("#d11f1f"),
                "Marketing" => Color.FromArgb("#d11f1f"),
                "Finance" => Color.FromArgb("#d11f1f"),
                "Startups" => Color.FromArgb("#d11f1f"),

                //Science
                "Research" => Color.FromArgb("#d11f1f"),
                "Physics" => Color.FromArgb("#d11f1f"),
                "Biology" => Color.FromArgb("#d11f1f"),
                "Chemistry" => Color.FromArgb("#d11f1f"),
                "Astronomy" => Color.FromArgb("#d11f1f"),

                _ => Colors.Gray


            };


        }
    }
}

<<<<<<< HEAD

=======
>>>>>>> eca5211 (Update Main_Page)
