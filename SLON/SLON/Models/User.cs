using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

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
        public new List<string> Urls { get; set; }
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

        private Color _cardColor = (Color)Application.Current.Resources["CardBackgroundColor"]; // исходный цвет
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


        public User(string username, string name, string surname, List<string> tags, string vocation, string info, string skills, List<string> urls)
        {
            Username = username;
            Name = name;
            Surname = surname;
            Tags = tags;
            Vocation = vocation;
            Info = info;
            Skills = skills;
            Urls = urls;
            _ = InitAvatarAsync(username);
        }

        private async Task<ImageSource> LoadAvatarSourceAsync(string username)
        {
            var uri = new Uri($"http://139.28.223.134:5000/photos/image/{Uri.EscapeDataString(username)}");
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                    return ImageSource.FromUri(uri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Avatar load error: {ex}");
            }
            return ImageSource.FromFile("default_profile_icon2.png");
        }


        public async Task InitAvatarAsync(string username)
        {
            Avatar = await LoadAvatarSourceAsync(username);


            // Инициализация цвета карточки
            UpdateCardColor();

            // Подписка на смену темы
            Application.Current.RequestedThemeChanged += (s, e) => UpdateCardColor();
        }

        public void UpdateCardColor()
        {
            // Используем TryGetValue для безопасного получения ресурса
            if (Application.Current.Resources.TryGetValue("CardBackgroundColor", out var color))
            {
                CardColor = (Color)color;
            }
            else
            {
                // Fallback-цвет, если ресурс не найден
                CardColor = Color.FromArgb("#292929");
            }
        }
    }
}