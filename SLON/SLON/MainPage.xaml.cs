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

        protected async override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            Users.Clear();

            for (int i = 0; i <= 50; i++)
            {
                User user = new(i, "Name #" + i, "tags", "vocation", "info", "skills");
                Users.Add(user); 
            }
        }

    }
}
