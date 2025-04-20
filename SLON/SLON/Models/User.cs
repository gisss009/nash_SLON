using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace SLON.Models
{
    public class User : INotifyPropertyChanged
    {
        public string Username { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }  // Добавлено поле фамилии

        public new List<string> Tags { get; set; }
        public string Vocation { get; set; }
        public string Info { get; set; }
        public string Skills { get; set; }

        public bool IsMutual { get; set; }
        public bool IsAcceptedMe { get; set; }
        public bool IsILikedHim { get; set; }

        private Color _cardColor = Color.FromArgb("#292929");
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
        public string FullName => $"{Name} {Surname}";


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Изменённый конструктор, который принимает фамилию
        public User(string username, string name, string surname, List<string> tags, string vocation, string info, string skills)
        {
            Username = username;
            Name = name;
            Surname = surname;
            Tags = tags;
            Vocation = vocation;
            Info = info;
            Skills = skills;
            IsMutual = false;
            IsAcceptedMe = false;
            IsILikedHim = false;
        }
    }
}
