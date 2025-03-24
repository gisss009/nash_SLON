using System.ComponentModel;

namespace SLON.Models
{
    public class User : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }       
        public new List<string> Tags { get; set; }       
        public string Vocation { get; set; }   
        public string Info { get; set; }     
        public string Skills { get; set; }     
        public bool IsMutual { get; set; }     // Если true - во "взаимных"
        public bool IsAcceptedMe { get; set; } // Принял ли он меня
        public bool IsILikedHim { get; set; }  // Лайкнул ли я его

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

        public User(int id, string name, List<string> tags, string vocation, string info, string skills)
        {
            Id = id;
            Name = name;
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
