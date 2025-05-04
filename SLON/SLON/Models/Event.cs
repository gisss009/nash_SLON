using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace SLON.Models
{
    public class Event : INotifyPropertyChanged
    {
        public string Hash { get; set; }
        public string Name { get; set; }
        public List<string> Categories { get; set; }
        public string Info { get; set; }
        public string Place { get; set; }
        public bool IsPublic { get; set; }
        public bool IsOnline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DateRange => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";

        public List<User> AddedParticipants { get; set; } = new List<User>();

        private Color _cardColor = (Color)Application.Current.Resources["CardBackgroundColor"];
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
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Event(string hash, string name, List<string> categories, string info, string place, bool isPublic, bool isOnline)
        {
            Hash = hash;
            Name = name;
            Categories = categories;
            Info = info;
            Place = place;
            IsPublic = isPublic;
            IsOnline = isOnline;
            // Инициализация цвета карточки
            UpdateCardColor();

            // Подписка на смену темы
            Application.Current.RequestedThemeChanged += (s, e) => UpdateCardColor();
        }

        public void UpdateCardColor()
        {
            if (Application.Current.Resources.TryGetValue("CardBackgroundColor", out var color))
            {
                CardColor = (Color)color;
            }
            else
            {
                // Fallback-цвет
                CardColor = Color.FromArgb("#292929");
            }
        }

    }
}
