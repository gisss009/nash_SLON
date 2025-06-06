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
            startDatePicker.DateSelected += OnFilterStartDateSelected;
            endDatePicker.DateSelected += OnFilterEndDateSelected;
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

        /// <summary>
        /// Срабатывает, когда пользователь меняет дату начала в фильтрах.
        /// Обновляет минимально допустимую дату конца и, если нужно, поднимает конец до начала.
        /// </summary>
        private void OnFilterStartDateSelected(object sender, DateChangedEventArgs e)
        {
            tempStartDate = e.NewDate;

            // Устанавливаем минимально допустимую дату в endDatePicker
            endDatePicker.MinimumDate = e.NewDate;

            // Если выбранная ранее tempEndDate была раньше нового начала — подтягиваем её
            if (endDatePicker.Date < e.NewDate)
                endDatePicker.Date = e.NewDate;
        }

        /// <summary>
        /// Срабатывает, когда пользователь меняет дату конца в фильтрах.
        /// Если новая дата < начала — отменяем выбор и показываем ошибку.
        /// </summary>
        private async void OnFilterEndDateSelected(object sender, DateChangedEventArgs e)
        {
            if (e.NewDate < startDatePicker.Date)
            {
                // Сообщаем об ошибке
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    "Дата окончания не может быть раньше даты начала.",
                    "OK");

                // Откатываем на максимально допустимую (равную началу)
                endDatePicker.Date = startDatePicker.Date;
            }
            else
            {
                tempEndDate = e.NewDate;
            }
        }


        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            // 1) Сохраняем даты
            Settings.SelectedEventStartDate = startDatePicker.Date;
            Settings.SelectedEventEndDate = endDatePicker.Date;

            // 2) Сохраняем категории
            Settings.selectedUserCategories = new HashSet<string>(tempSelectedUserCategories);
            Settings.selectedEventCategories = new HashSet<string>(tempSelectedEventCategories);

            // 3) Сохраняем флаги Online/Public
            Settings.SelectedEventIsOnline = tempSelectedEventIsOnline;
            Settings.SelectedEventIsPublic = tempSelectedEventIsPublic;

            Settings.Save();

            // 4) Перезапускаем фильтрацию **до** закрытия попапа
            mainPage.FilterCards();

            // 5) Закрываем попап
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
                        OfflineEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorSettingsMain"];
                        tempSelectedEventIsOnline = true;
                        break;
                    case "Offline":
                        OfflineEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                        OnlineEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorSettingsMain"];
                        tempSelectedEventIsOnline = false;
                        break;
                    case "Public":
                        PublicEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                        PrivateEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorSettingsMain"];
                        tempSelectedEventIsPublic = true;
                        break;
                    case "Private":
                        PrivateEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorPurpleMain"];
                        PublicEvent.BackgroundColor = (Color)Application.Current.Resources["ButtonColorSettingsMain"];
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
