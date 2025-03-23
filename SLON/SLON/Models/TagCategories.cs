namespace SLON.Models
{
    internal class TagCategories
    {
        public static Dictionary<string, List<string>> Categories { get; } = new Dictionary<string, List<string>>
        {
            { "Creation", new List<string> { "Art", "Design", "Innovation", "Creativity", "Prototyping" } },
            { "Education", new List<string> { "Learning", "Teaching", "Courses", "Workshops", "Tutoring" } },
            { "IT", new List<string> { "Programming", "Cybersecurity", "AI", "Cloud Computing" } },
            { "Social", new List<string> { "Community", "Networking", "Events", "Charity", "Social Media" } },
            { "Business", new List<string> { "Entrepreneurship", "Management", "Marketing", "Finance", "Startups" } },
            { "Science", new List<string> { "Research", "Physics", "Biology", "Chemistry", "Astronomy" } },
            { "Sport", new List<string> { "Football", "Basketball", "Tennis", "Running", "Cycling" } },
            { "Health", new List<string> { "Fitness", "Nutrition", "Wellness", "Yoga", "Medicine" } }
        };
    }
}
