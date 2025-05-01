using CommunityToolkit.Maui.Views;
using SLON.Models;
using SLON.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace SLON
{
    public partial class AddUsersToEventPopup : Popup
    {
        private Event _event;
        private ObservableCollection<UserSelectable> _mutualUsers;

        public AddUsersToEventPopup(Event ev)
        {
            InitializeComponent();
            _event = ev;
            EventNameLabel.Text = $"Event: {ev.Name}";
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // 1) Получаем детали события (включая members)
            var data = await AuthService.GetEventDetailsAsync(_event.Hash);
            if (data?.members != null)
            {
                _event.AddedParticipants.Clear();
                foreach (var username in data.members)
                {
                    // Для каждого юзера подтягиваем профиль и строим модель User
                    var profile = await AuthService.GetUserProfileAsync(username);
                    if (profile != null)
                    {
                        var tagsList = profile.tags != null
                            ? profile.tags.Values
                                .SelectMany(t => t.Split(','))
                                .Select(x => x.Trim())
                                .Where(x => !string.IsNullOrEmpty(x))
                                .ToList()
                            : new List<string>();

                        var skillsList = profile.skills != null
                            ? profile.skills.Values.ToList()
                            : new List<string>();

                        var userModel = new User(
                            profile.username,
                            profile.name,
                            profile.surname,
                            tagsList,
                            profile.vocation,
                            profile.description,
                            string.Join(", ", skillsList)
                        );
                        _event.AddedParticipants.Add(userModel);
                    }
                }
            }
            await LoadMutualUsers();
        }

        private async Task LoadMutualUsers()
        {
            // 2) Разбиваем mutual-пользователей на две группы
            var notAdded = new List<UserSelectable>();
            var already = new List<UserSelectable>();

            foreach (var user in Favourites.mutual)
            {
                bool isAdded = _event.AddedParticipants.Any(u => u.Username == user.Username);
                var item = new UserSelectable
                {
                    User = user,
                    Name = user.FullName,
                    IsAlreadyAdded = isAdded,
                    IsSelected = isAdded
                };

                if (isAdded) already.Add(item);
                else notAdded.Add(item);
            }

            // Сначала не добавленные, потом уже добавленные
            _mutualUsers = new ObservableCollection<UserSelectable>(notAdded.Concat(already));
            UsersCollectionView.ItemsSource = _mutualUsers;
        }

        private async void OnAddButtonClicked(object sender, EventArgs e)
        {
            // 3) Выбираем только новых отмеченных
            var toAdd = _mutualUsers
                .Where(x => x.IsSelected && !x.IsAlreadyAdded)
                .Select(x => x.User)
                .ToList();

            foreach (var user in toAdd)
            {
                // a) добавить в members события
                bool okEv = await AuthService.AddEventMemberAsync(user.Username, _event.Hash);
                // b) добавить ивент в профиль пользователя
                bool okProf = await AuthService.AddProfileEventAsync(user.Username, _event.Hash);

                if (okEv && okProf)
                {
                    _event.AddedParticipants.Add(user);
                }
            }

            // 4) Перезагружаем список, чтобы новые добавленные ушли вниз и были с галочкой
            await LoadMutualUsers();

            await Application.Current.MainPage.DisplayAlert(
                "Added",
                $"Добавлено: {string.Join(", ", toAdd.Select(u => u.FullName))}",
                "OK"
            );
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
            => Close();
    }

    public class UserSelectable
    {
        public User User { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public bool IsAlreadyAdded { get; set; }
        public bool IsCheckboxEnabled => !IsAlreadyAdded;
    }
}
