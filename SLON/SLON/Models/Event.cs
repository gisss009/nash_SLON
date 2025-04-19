using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace SLON.Models
{
    public class Event : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Categories { get; set; }
        public string Info { get; set; }
        public string Place { get; set; }
        public bool IsPublic { get; set; }
        public bool IsOnline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<User> AddedParticipants { get; set; } = new();
        public string DateRange => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";

        private Color _cardColor;
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

        public Event(int id, string name, List<string> categories, string info, string place, bool is_public, bool is_online)
        {
            Id = id;
            Name = name;
            Categories = categories;
            Info = info;
            Place = place;
            IsPublic = is_public;
            IsOnline = is_online;

            UpdateCardColor();
            Application.Current.RequestedThemeChanged += (s, e) => UpdateCardColor();
        }

        private void UpdateCardColor()
        {
            if (Application.Current.Resources.TryGetValue("CardBackgroundColor", out var color))
            {
                CardColor = (Color)color;
            }
            else
            {
                // Fallback цвета если ресурс не найден
                CardColor = Application.Current.UserAppTheme == AppTheme.Dark
                    ? Color.FromArgb("#292929")
                    : Colors.White;
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}