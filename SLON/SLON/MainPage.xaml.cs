using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Plugin.Maui.SwipeCardView;
using Plugin.Maui.SwipeCardView.Core;
using SLON.Models;
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

        public MainPage()
        {
            InitializeComponent();
            OnCardSwipedCommand = new Command<SwipedCardEventArgs>(OnCardSwiped);
            BindingContext = this;
        }

        private void OnCardSwiped(SwipedCardEventArgs e)
        {
            var item = e.Item as User;

            if (e.Direction == SwipeCardDirection.Left)
            {
                Favourites.favorites.Add(item);
                //    Favourites.favorites.Add(new Event(
                //    swipedUser.Id,
                //    swipedUser.Name,
                //    swipedUser.Tags,
                //    swipedUser.Info,
                //    "No location specified"
                //));
            }
        }

        protected /*async*/ override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            Users.Clear();
            Events.Clear();

            FillUserCards();
            FillEventsCards();
        }

        public void FillUserCards()
        {
            Users.Add(new User(1, "Alice Johnson", "IT, Creation", "UI Designer", "Specialist in mobile app design", "Adobe XD, Figma, Photoshop"));
            Users.Add(new User(2, "Bob Smith", "IT", "Backend Developer", "Focused on high-performance APIs", "C#, .NET, SQL"));
            Users.Add(new User(3, "Carla Perez", "IT, Creation", "Frontend Developer", "React expert with a focus on responsive design", "JavaScript, React, CSS"));
            Users.Add(new User(4, "David Lee", "IT, Science", "Data Scientist", "Experienced in AI and machine learning", "Python, TensorFlow, PyTorch"));
            Users.Add(new User(5, "Emma Brown", "Business, Creation", "Marketing Manager", "Specialist in social media campaigns", "SEO, Content Marketing, Google Ads"));
            Users.Add(new User(6, "Frank Wilson", "IT, Business", "DevOps Engineer", "Focus on CI/CD pipelines", "Docker, Kubernetes, Jenkins"));
            Users.Add(new User(7, "Grace Adams", "Business, Education", "Project Manager", "Certified Scrum Master", "Agile, Scrum, Kanban"));
            Users.Add(new User(8, "Henry Carter", "IT, Business", "Blockchain Developer", "Expert in smart contracts", "Solidity, Ethereum, Web3"));
            Users.Add(new User(9, "Ivy Martinez", "Creation, Education", "Content Writer", "Crafts engaging stories", "Copywriting, Blogging, SEO Writing"));
            Users.Add(new User(10, "Jack Robinson", "IT, Science", "Cybersecurity Specialist", "Focus on network security", "Penetration Testing, Firewalls, Ethical Hacking"));
        }

        public void FillEventsCards()
        {
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
    }
}
