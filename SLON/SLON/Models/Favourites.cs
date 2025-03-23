using System.Collections.ObjectModel;

namespace SLON.Models
{
    public static class Favourites
    {
        // Все пользователи, которых я лайкнул (All)
        public static ObservableCollection<User> favorites { get; set; } = new();

        public static ObservableCollection<Event> favoriteEvents { get; set; } = new();

        // Список взаимных лайков (Mutual)
        public static ObservableCollection<User> mutual { get; set; } = new();

        // Список тех, кто меня запросил (Requests)
        public static ObservableCollection<User> requests { get; set; } = new();

        // Список тех, кто меня уже принял (Accepted)
        public static ObservableCollection<User> accepted { get; set; } = new();

        private static bool _initialized = false;

        static Favourites()
        {
            InitializeTestData();
        }

        private static void InitializeTestData()
        {
            if (_initialized) return;
            _initialized = true;

            // Пример: Пользователи, которые уже меня приняли (Accepted)
            // Считаем, что у нас уже есть взаимный лайк (Mutual)
            var userA = new User(101, "Ivan Petrov", new List<string> { "IT", "Business" }, "DevOps Engineer", "Some info about Ivan", "Docker, Kubernetes")
            {
                IsAcceptedMe = true,
                IsMutual = true      
            };
            var userB = new User(102, "Anna Kim", new List<string> { "Creation", "Education" }, "Content Writer", "Some info about Anna", "Copywriting, Blogging")
            {
                IsAcceptedMe = true,
                IsMutual = true
            };
            var userC = new User(103, "Zoe Carter", new List<string> { "Business" }, "Marketing Manager", "Some info about Zoe", "SMM, SEO")
            {
                IsAcceptedMe = true,
                IsMutual = true
            };

            accepted.Add(userA);
            accepted.Add(userB);
            accepted.Add(userC);

            mutual.Add(userA);
            mutual.Add(userB);
            mutual.Add(userC);

            // Пример: Пользователи, которые меня запросили (Requests)
            // Они хотят, чтобы я их принял (Accept).
            var userR1 = new User(201, "Peter Johnson", new List<string> { "Business", "Education" }, "Project Manager", "Some info about Peter", "Scrum, Agile")
            {
                IsILikedHim = false // он меня лайкнул, а я его нет
            };
            var userR2 = new User(202, "Elena Wilson", new List<string> { "IT", "Creation" }, "UI/UX Designer", "Some info about Elena", "Figma, Photoshop")
            {
                IsILikedHim = false
            };
            var userR3 = new User(203, "Tom Jordan", new List<string> { "IT" }, "Backend Dev", "Some info about Tom", "C#, .NET, SQL")
            {
                IsILikedHim = false
            };

            requests.Add(userR1);
            requests.Add(userR2);
            requests.Add(userR3);
        }
    }
}
