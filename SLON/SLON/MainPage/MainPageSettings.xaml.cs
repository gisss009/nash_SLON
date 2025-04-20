using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Layouts;
using SLON.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SLON
{
    public partial class MainPageSettings : Popup
    {
        // ��������� ��������� ����� ��������
        private HashSet<string> tempSelectedUserCategories;
        private HashSet<string> tempSelectedEventCategories;
        private bool? tempSelectedEventIsOnline;
        private bool? tempSelectedEventIsPublic;
        private DateTime tempStartDate;
        private DateTime tempEndDate;

        public ObservableCollection<string> categories { get; set; } = new();
        private MainPage mainPage;

        public MainPageSettings(MainPage mainPage)
        {
            InitializeComponent();
            this.mainPage = mainPage;

            // �������������� ��������� ����� �� ���������� ��������
            tempSelectedUserCategories = new HashSet<string>(Settings.selectedUserCategories);
            tempSelectedEventCategories = new HashSet<string>(Settings.selectedEventCategories);
            tempSelectedEventIsOnline = Settings.SelectedEventIsOnline;
            tempSelectedEventIsPublic = Settings.SelectedEventIsPublic;
            tempStartDate = Settings.SelectedEventStartDate ?? DateTime.Today;
            tempEndDate = Settings.SelectedEventEndDate ?? DateTime.Today;

            // ������������� ��������� ����
            startDatePicker.Date = tempStartDate;
            endDatePicker.Date = tempEndDate;

            // ���������� ���� �������� ��� ������� ������ ���� ������ ����� �������
            ForEvent.IsVisible = (mainPage.ProfilesEventsButtonStatus == 1);

            // ��������� ������ ���������
            categories.Add("IT");
            categories.Add("Creation");
            categories.Add("Sport");
            categories.Add("Science");
            categories.Add("Business");
            categories.Add("Education");
            categories.Add("Social");
            categories.Add("Health");

            categoriesContainer.Children.Clear();
            foreach (var category in categories)
            {
                var button = new Button
                {
                    Text = category,
                    TextColor = Colors.White,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 20,
                    HeightRequest = 50,
                    Padding = new Thickness(10, 5),
                    Margin = new Thickness(5),
                    BorderWidth = 1,
                    BackgroundColor = (mainPage.ProfilesEventsButtonStatus == 0
            ? (tempSelectedUserCategories.Contains(category)
                ? GetCategoryColor(category)
                : (Color)Application.Current.Resources["ButtonCatsColor"])
            : (tempSelectedEventCategories.Contains(category)
                ? GetCategoryColor(category)
                : (Color)Application.Current.Resources["ButtonCatsColor"]))
                };

                button.Clicked += OnCategorySelected;
                FlexLayout.SetBasis(button, new FlexBasis(0.48f, true));
                categoriesContainer.Children.Add(button);
            }
            if (categories.Count % 2 != 0)
            {
                var spacer = new BoxView
                {
                    WidthRequest = 0,
                    HeightRequest = 70,
                    Margin = new Thickness(5)
                };
                FlexLayout.SetBasis(spacer, new FlexBasis(0.48f, true));
                categoriesContainer.Children.Add(spacer);
            }

            // ��������������� ����������� ��������� ��� �������
            if (mainPage.ProfilesEventsButtonStatus == 1)
            {
                if (tempSelectedEventIsOnline.HasValue)
                {
                    if (tempSelectedEventIsOnline.Value)
                    {
                        OnlineEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                        OfflineEvent.BackgroundColor = Color.FromArgb("#292929");
                    }
                    else
                    {
                        OfflineEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                        OnlineEvent.BackgroundColor = Color.FromArgb("#292929");
                    }
                }
                if (tempSelectedEventIsPublic.HasValue)
                {
                    if (tempSelectedEventIsPublic.Value)
                    {
                        PublicEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                        PrivateEvent.BackgroundColor = Color.FromArgb("#292929");
                    }
                    else
                    {
                        PrivateEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                        PublicEvent.BackgroundColor = Color.FromArgb("#292929");
                    }
                }
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            // ��������� ��������� ���� � ���������� ��������� (������ ��� �������)
            if (mainPage.ProfilesEventsButtonStatus == 1)
            {
                Settings.SelectedEventStartDate = startDatePicker.Date;
                Settings.SelectedEventEndDate = endDatePicker.Date;
            }
            // ��������� ��������� ��������� �� ���������� � ����� �������
            Settings.selectedUserCategories = new HashSet<string>(tempSelectedUserCategories);
            Settings.selectedEventCategories = new HashSet<string>(tempSelectedEventCategories);
            Settings.SelectedEventIsOnline = tempSelectedEventIsOnline;
            Settings.SelectedEventIsPublic = tempSelectedEventIsPublic;
            mainPage.FilterCards();
            Close();
        }

        private void OnEventTypeClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                switch (button.Text)
                {
                    case "Online":
                        OnlineEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                        OfflineEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];
                        tempSelectedEventIsOnline = true;
                        break;
                    case "Offline":
                        OfflineEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                        OnlineEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];
                        tempSelectedEventIsOnline = false;
                        break;
                    case "Public":
                        PublicEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                        PrivateEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];
                        tempSelectedEventIsPublic = true;
                        break;
                    case "Private":
                        PrivateEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                        PublicEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];
                        tempSelectedEventIsPublic = false;
                        break;
                }
            }
        }

        private void OnCategorySelected(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var category = button.Text;
                if (mainPage.ProfilesEventsButtonStatus == 0)
                {
                    if (!tempSelectedUserCategories.Contains(category))
                    {
                        button.BackgroundColor = GetCategoryColor(category);
                        tempSelectedUserCategories.Add(category);
                    }
                    else
                    {
                        button.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];
                        tempSelectedUserCategories.Remove(category);
                    }
                    Debug.WriteLine("Temp selected user categories: " + string.Join(", ", tempSelectedUserCategories));
                }
                else if (mainPage.ProfilesEventsButtonStatus == 1)
                {
                    if (!tempSelectedEventCategories.Contains(category))
                    {
                        button.BackgroundColor = GetCategoryColor(category);
                        tempSelectedEventCategories.Add(category);
                    }
                    else
                    {
                        button.BackgroundColor = (Color)Application.Current.Resources["ButtonColorMain"];
                        tempSelectedEventCategories.Remove(category);
                    }
                    Debug.WriteLine("Temp selected event categories: " + string.Join(", ", tempSelectedEventCategories));
                }
            }
        }

        private Color GetCategoryColor(string category)
        {
            return category switch
            {
                "IT" => Color.FromArgb("#3541DC"),
                "Creation" => Color.FromArgb("#0A6779"),
                "Sport" => Color.FromArgb("#A92123"),
                "Science" => Color.FromArgb("#038756"),
                "Business" => Color.FromArgb("#640693"),
                "Education" => Color.FromArgb("#B55E24"),
                "Social" => Color.FromArgb("#FF6F61"),
                "Health" => Color.FromArgb("#6B5B95"),
                _ => Colors.Gray
            };
        }
    }
}
