using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls;
using SLON.Models;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;

namespace SLON;

public partial class MainPageSettings : Popup
{
    public ObservableCollection<string> categories { get; set; } = new();
    private MainPage mainPage;

    public MainPageSettings(MainPage mainPage)
    {
        InitializeComponent();
        this.mainPage = mainPage;
        ForEvent.IsVisible = (mainPage.ProfilesEventsButtonStatus == 1);

        // Загружаем категории в зависимости от активного фильтра
        if (mainPage.ProfilesEventsButtonStatus == 0) // профили
        {
            categories.Add("IT");
            categories.Add("Creation");
            categories.Add("Sport");
            categories.Add("Science");
            categories.Add("Business");
            categories.Add("Education");
        }
        else if (mainPage.ProfilesEventsButtonStatus == 1) // события (events)
        {
            categories.Add("IT");
            categories.Add("Creation");
            categories.Add("Sport");
            categories.Add("Science");
            categories.Add("Business");
            categories.Add("Education");


        }

        // Создание FlexLayout, динамическое создание кнопок и т.д.
        var flexLayout = new FlexLayout
        {
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
            JustifyContent = FlexJustify.SpaceEvenly
        };

        var categoriesLayout = new VerticalStackLayout { Spacing = 10 };


        // Очищаем контейнер перед добавлением новых элементов
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
                HeightRequest = 70,
                Padding = new Thickness(10, 5),
                Margin = new Thickness(5),
                BorderColor = Colors.LightGray,
                BorderWidth = 1,
                BackgroundColor = (mainPage.ProfilesEventsButtonStatus == 0
                                   ? Settings.selectedUserCategories.Contains(category)
                                   : Settings.selectedEventCategories.Contains(category))
                                   ? GetCategoryColor(category)
                                   : Color.FromRgb(129, 129, 129)
            };

            button.Clicked += OnCategorySelected;

            // Устанавливаем кнопке базовый размер (50% ширины контейнера)
            FlexLayout.SetBasis(button, new FlexBasis(0.48f, true));

            categoriesContainer.Children.Add(button);
        }

        // Если категорий нечетное количество, добавляем пустой BoxView
        if (categories.Count % 2 != 0)
        {
            var spacer = new BoxView
            {
                WidthRequest = 0, // Минимальная ширина
                HeightRequest = 70, // Делаем высоту такой же, как у кнопок
                Margin = new Thickness(5)
            };

            FlexLayout.SetBasis(spacer, new FlexBasis(0.48f, true)); // Занимает место второй колонки

            categoriesContainer.Children.Add(spacer);
        }


        //// Если категорий нечетное количество, добавляем пустой BoxView
        //if (categories.Count % 2 != 0)
        //{
        //    var spacer = new BoxView
        //    {
        //        WidthRequest = 0, // Минимальная ширина
        //        HeightRequest = 70, // Делаем высоту такой же, как у кнопок
        //        Margin = new Thickness(5)
        //    };

        //    FlexLayout.SetBasis(spacer, new FlexBasis(0.48f, true)); // Занимает место второй колонки

        //    categoriesContainer.Children.Add(spacer);
        //}



        // Добавляем прокручиваемый список категорий в контейнер
        //categoriesContainer.Children.Add(categoriesLayout);


        //var scrollView = new ScrollView
        //{
        //    Orientation = ScrollOrientation.Vertical,
        //    Content = flexLayout
        //};

        //if (verticalStackLayout != null)
        //    verticalStackLayout.Children.Insert(1, scrollView);
    }


    private void OnCloseClicked(object sender, EventArgs e)
    {
        // Сохраняем выбранные категории в Settings
        mainPage.FilterCards();
        Close();
    }
    private void OnEventTypeClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            // Пример логики для изменения цвета кнопок
            switch (button.Text)
            {
                case "Online":
                    OnlineEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                    OfflineEvent.BackgroundColor = Color.FromArgb("#292929");
                    break;
                case "Offline":
                    OfflineEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                    OnlineEvent.BackgroundColor = Color.FromArgb("#292929");
                    break;
                case "Public":
                    PublicEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                    PrivateEvent.BackgroundColor = Color.FromArgb("#292929");
                    break;
                case "Private":
                    PrivateEvent.BackgroundColor = Color.FromArgb("#8E44AD");
                    PublicEvent.BackgroundColor = Color.FromArgb("#292929");
                    break;
            }
        }
    }

    private void OnCategorySelected(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var category = button.Text;

            if (mainPage.ProfilesEventsButtonStatus == 0) // профили
            {
                if (!Settings.selectedUserCategories.Contains(category))
                {
                    button.BackgroundColor = GetCategoryColor(category);
                    Settings.selectedUserCategories.Add(category);
                }
                else
                {
                    button.BackgroundColor = Color.FromRgb(129, 129, 129);
                    Settings.selectedUserCategories.Remove(category);
                }
                Debug.WriteLine("Выбранные категории профилей: " + string.Join(", ", Settings.selectedUserCategories));
            }
            else if (mainPage.ProfilesEventsButtonStatus == 1) // события
            {
                if (!Settings.selectedEventCategories.Contains(category))
                {
                    button.BackgroundColor = GetCategoryColor(category);
                    Settings.selectedEventCategories.Add(category);
                }
                else
                {
                    button.BackgroundColor = Color.FromRgb(129, 129, 129);
                    Settings.selectedEventCategories.Remove(category);
                }
                Debug.WriteLine("Выбранные категории событий: " + string.Join(", ", Settings.selectedEventCategories));
            }
        }
    }


    // Функция для получения цвета категории
    private Color GetCategoryColor(string category)
    {
        return category switch
        {

            "Creation" => Color.FromArgb("#008f39"),
            "Education" => Color.FromArgb("#a020f0"),
            "IT" => Color.FromArgb("#00a6ff"),
            "Social" => Color.FromArgb("#d11f1f"),
            "Business" => Color.FromArgb("#d11f1f"),
            "Science" => Color.FromArgb("#d11f1f"),


            //Creation
            "Art" => Color.FromArgb("#008f39"),
            "Design" => Color.FromArgb("#008f39"),
            "Innovation" => Color.FromArgb("#008f39"),
            "Creativity" => Color.FromArgb("#008f39"),
            "Prototyping" => Color.FromArgb("#008f39"),

            //"Education"
            "Learning" => Color.FromArgb("#a020f0"),
            "Teaching" => Color.FromArgb("#a020f0"),
            "Courses" => Color.FromArgb("#a020f0"),
            "Workshops" => Color.FromArgb("#a020f0"),
            "Tutoring" => Color.FromArgb("#a020f0"),

            // IT
            "Programming" => Color.FromArgb("#00a6ff"),
            "Cybersecurity" => Color.FromArgb("#00a6ff"),
            "AI" => Color.FromArgb("#00a6ff"),
            "Cloud Computing" => Color.FromArgb("#00a6ff"),

            //Social
            "Community" => Color.FromArgb("#d11f1f"),
            "Networking" => Color.FromArgb("#d11f1f"),
            "Events" => Color.FromArgb("#d11f1f"),
            "Charity" => Color.FromArgb("#d11f1f"),
            "Social Media" => Color.FromArgb("#d11f1f"),

            //Business

            "Entrepreneurship" => Color.FromArgb("#d11f1f"),
            "Management" => Color.FromArgb("#d11f1f"),
            "Marketing" => Color.FromArgb("#d11f1f"),
            "Finance" => Color.FromArgb("#d11f1f"),
            "Startups" => Color.FromArgb("#d11f1f"),

            //Science
            "Research" => Color.FromArgb("#d11f1f"),
            "Physics" => Color.FromArgb("#d11f1f"),
            "Biology" => Color.FromArgb("#d11f1f"),
            "Chemistry" => Color.FromArgb("#d11f1f"),
            "Astronomy" => Color.FromArgb("#d11f1f"),

            _ => Colors.Gray
        };
    }
}
