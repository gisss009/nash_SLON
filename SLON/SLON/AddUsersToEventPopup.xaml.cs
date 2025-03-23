using CommunityToolkit.Maui.Views;
using SLON.Models;
using System;
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
            LoadMutualUsers();
        }

        private void LoadMutualUsers()
        {
            _mutualUsers = new ObservableCollection<UserSelectable>();

            var addedUsers = _event.AddedParticipants.ToList();
            var notAddedUsers = Favourites.mutual.Except(addedUsers).ToList();

            foreach (var user in notAddedUsers)
            {
                _mutualUsers.Add(new UserSelectable
                {
                    User = user,
                    Name = user.Name,
                    IsSelected = false,
                    IsAlreadyAdded = false
                });
            }

            foreach (var user in addedUsers)
            {
                _mutualUsers.Add(new UserSelectable
                {
                    User = user,
                    Name = user.Name,
                    IsSelected = true,
                    IsAlreadyAdded = true
                });
            }

            UsersCollectionView.ItemsSource = _mutualUsers;
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        private async void OnAddButtonClicked(object sender, EventArgs e)
        {
            var selectedUsers = _mutualUsers
                .Where(u => u.IsSelected && !u.IsAlreadyAdded)
                .Select(u => u.User)
                .ToList();

            if (!selectedUsers.Any())
            {
                Close();
                return;
            }

            _event.AddedParticipants.AddRange(selectedUsers);
            string names = string.Join(", ", selectedUsers.Select(u => u.Name));

            await Application.Current.MainPage.DisplayAlert("Added",$"Пользователи [{names}] добавлены в группу ивента \"{_event.Name}\"", "OK");
            Close();
        }
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
