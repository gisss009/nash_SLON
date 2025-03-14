using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Plugin.Maui.SwipeCardView;
using Plugin.Maui.SwipeCardView.Core;
using SLON.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            Users.Add(new User(2, "Bob Smith", "IT", "Backend Developer", "High-performance APIs", "C#, .NET, SQL"));
        }

        public void FillEventsCards()
        {
<<<<<<< Updated upstream
            Events.Add(new Event(1, "Tech Conference 2024", "Technology, Mobile, UI/UX", "A conference on the latest trends in mobile app design, APIs, and UI/UX", "Ulitsa 2-ya Krivorozhskaya, Rostov-on-Don"));
            Events.Add(new Event(2, "Backend Development Workshop", "Backend, APIs, Performance", "A hands-on workshop focusing on building high-performance APIs", "Ulitsa Budennovskiy, Rostov-on-Don"));
            Events.Add(new Event(3, "React Masterclass", "React, Frontend, Responsive Design", "An in-depth session on mastering React and responsive design", "Ulitsa Berzhaninova, Rostov-on-Don"));
            Events.Add(new Event(4, "AI and Machine Learning Summit", "AI, Machine Learning, Technology", "A summit dedicated to AI advancements and machine learning applications", "Ulitsa Chekistov, Rostov-on-Don"));
            Events.Add(new Event(5, "Social Media Marketing Bootcamp", "Marketing, Social Media, Campaigns", "A bootcamp focused on building effective social media campaigns", "Ulitsa Pushkinskaya, Rostov-on-Don"));
            Events.Add(new Event(6, "DevOps and Automation Training", "DevOps, CI/CD, Automation", "A training session on CI/CD pipelines and DevOps tools", "Ulitsa Sovetskaya, Rostov-on-Don"));
            Events.Add(new Event(7, "Agile Project Management Seminar", "Agile, Scrum, Kanban", "A seminar on Agile methodologies, Scrum, and Kanban", "Ulitsa Lenina, Rostov-on-Don"));
            Events.Add(new Event(8, "Blockchain & Crypto Conference", "Blockchain, Cryptocurrency, Smart Contracts", "A conference focused on the latest in blockchain technology and smart contracts", "Ulitsa Maksima Gorkogo, Rostov-on-Don"));
            Events.Add(new Event(9, "Content Writing for SEO", "SEO, Content Writing, Blogging", "A workshop on creating SEO-friendly content for blogs and websites", "Ulitsa Nekrasova, Rostov-on-Don"));
            Events.Add(new Event(10, "Cybersecurity Awareness Workshop", "Cybersecurity, Networking, Ethical Hacking", "A workshop on network security and ethical hacking", "Ulitsa Molodezhnyy, Rostov-on-Don"));
=======
            Events.Add(new Event(1, "Tech Conference 2024", "IT, Education", "A conference on the latest trends", "Rostov-on-Don", true, true));
            Events.Add(new Event(2, "Conference 2025", "IT", "A conference on the latest trends", "Rostov-on-Don", false, false));
            OnPropertyChanged(nameof(Events));
>>>>>>> Stashed changes
        }

        private void OnCardSwiped(SwipedCardEventArgs e)
        {
            if (e.Item is User swipedUser)
            {
                if (e.Direction == SwipeCardDirection.Left)
                {
                    Debug.WriteLine($"Liked: {swipedUser.Name}");

                    if (!Favourites.favorites.Any(u => u.Id == swipedUser.Id))
                    {
                        swipedUser.IsILikedHim = true;
                        Favourites.favorites.Add(swipedUser);
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


        public void FilterCards()
        {
            // Логика фильтрации
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

            swipeCardView.IsVisible = false;
            swipeCardViewEvent.IsVisible = true;
        }

        public void OnProfilesButtonClicked(object sender, EventArgs e)
        {
            if (ProfilesEventsButtonStatus != 1)
                return;

            ProfilesEventsButtonStatus = 0;

            EventsButton.BackgroundColor = Colors.Black;
            ProfilesButton.BackgroundColor = Colors.Grey;

            swipeCardView.IsVisible = true;
            swipeCardViewEvent.IsVisible = false;
        }
    }
}
