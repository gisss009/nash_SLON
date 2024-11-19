using SLON.Models;
using System.Collections.ObjectModel;

namespace SLON
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<User> Users { get; set; } = new();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected /*async*/ override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            Users.Clear();

            Users.Add(new User(1, "Alice Johnson", "design, UI/UX", "UI Designer", "Specialist in mobile app design", "Adobe XD, Figma, Photoshop"));
            Users.Add(new User(2, "Bob Smith", "backend, API", "Backend Developer", "Focused on high-performance APIs", "C#, .NET, SQL"));
            Users.Add(new User(3, "Carla Perez", "frontend, React", "Frontend Developer", "React expert with a focus on responsive design", "JavaScript, React, CSS"));
            Users.Add(new User(4, "David Lee", "AI, ML", "Data Scientist", "Experienced in AI and machine learning", "Python, TensorFlow, PyTorch"));
            Users.Add(new User(5, "Emma Brown", "marketing, social media", "Marketing Manager", "Specialist in social media campaigns", "SEO, Content Marketing, Google Ads"));
            Users.Add(new User(6, "Frank Wilson", "DevOps, automation", "DevOps Engineer", "Focus on CI/CD pipelines", "Docker, Kubernetes, Jenkins"));
            Users.Add(new User(7, "Grace Adams", "project management, Agile", "Project Manager", "Certified Scrum Master", "Agile, Scrum, Kanban"));
            Users.Add(new User(8, "Henry Carter", "blockchain, crypto", "Blockchain Developer", "Expert in smart contracts", "Solidity, Ethereum, Web3"));
            Users.Add(new User(9, "Ivy Martinez", "copywriting, storytelling", "Content Writer", "Crafts engaging stories", "Copywriting, Blogging, SEO Writing"));
            Users.Add(new User(10, "Jack Robinson", "cybersecurity, networks", "Cybersecurity Specialist", "Focus on network security", "Penetration Testing, Firewalls, Ethical Hacking"));

            //for (int i = 0; i <= 50; i++)
            //{
            //    User user = new(i, "Name #" + i, "tags", "vocation", "info", "skills");
            //    Users.Add(user);
            //}
        }

    }
}
