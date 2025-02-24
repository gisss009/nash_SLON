using CommunityToolkit.Maui.Core;
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

        // profiles = 0; events = 1
        public int ProfilesEventsButtonStatus = 0;

        public MainPage()
        {
            InitializeComponent();
            OnCardSwipedCommand = new Command<SwipedCardEventArgs>(OnCardSwiped);
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

            var usersToAdd = Users.Where(user => !Favourites.favorites.Any(fav => fav.Id == user.Id)).ToList();
            foreach (var user in usersToAdd)
            {
                Users.Add(user);
            }
        }

        public void FillEventsCards()
        {
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

        }

        private void OnCardSwiped(SwipedCardEventArgs e)
        {
            if (e.Item is User swipedUser)
            {
                if (e.Direction == SwipeCardDirection.Left)
                {
                    Debug.WriteLine($"Liked: {swipedUser.Name}");
                    Debug.WriteLine($"count: {Favourites.favorites.Count}");

                    if (!Favourites.favorites.Any(u => u.Id == swipedUser.Id))
                    {
                        Favourites.favorites.Add(swipedUser);
                    }
                }
                else if (e.Direction == SwipeCardDirection.Right)
                {
                    Debug.WriteLine($"Skipped: {swipedUser.Name}");
                }
            }
            else
            {
                Debug.WriteLine("Swiped item is not a User");
            }
        }

        public void FilterCards()
        {
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
                }
            }
        }

        private void OnButtonSettingsClicked(object sender, System.EventArgs e)
        {
            this.ShowPopupAsync(new MainPageSettings(this));
        }

        public void OnEventsButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus != 0)
                return;

            ProfilesEventsButtonStatus = 1;

            EventsButton.BackgroundColor = Colors.Grey;
            ProfilesButton.BackgroundColor = Colors.Black;

            swipeCardView.IsVisible = true;
        }

        public void OnProfilesButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus != 1)
                return;

            ProfilesEventsButtonStatus = 0;

            EventsButton.BackgroundColor = Colors.Black;
            ProfilesButton.BackgroundColor = Colors.Grey;

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
    }
}


