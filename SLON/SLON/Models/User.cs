using System.ComponentModel;

namespace SLON.Models
{
    public class User : INotifyPropertyChanged
    {
        public string Username { get; set; }
        public string Name { get; set; }       
        public string Surname { get; set; }
        public new List<string> Tags { get; set; }       
        public string Vocation { get; set; }   
        public string Info { get; set; }     
        public string Skills { get; set; }
        public string FullName => $"{Name} {Surname}";

        // Новый параметр для аватарки
        private ImageSource _avatar;
        public ImageSource Avatar
        {
            get => _avatar;
            set
            {
                if (_avatar != value)
                {
                    _avatar = value;
                    OnPropertyChanged(nameof(Avatar));
                }

            }
        }

        private Color _cardColor = Color.FromArgb("#292929"); // исходный цвет
        public Color CardColor
        {
            get => _cardColor;
            set
            {
                if (_cardColor != value)
                {
                    _cardColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public User()
        {
        }


        public User(string username, string name, string surname, List<string> tags, string vocation, string info, string skills)
        {
            Username = username;
            Name = name;
            Surname = surname;
            Tags = tags;
            Vocation = vocation;
            Info = info;
            Skills = skills;
            Avatar = ImageSource.FromFile("avatar_placeholder.png");
        }

    }
}
