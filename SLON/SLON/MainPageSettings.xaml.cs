using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using SLON.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;

namespace SLON;

public partial class MainPageSettings : Popup
{
    public ObservableCollection<string> categories { get; set; } = new();
    //public HashSet<string> selectedCategories { get; set; } = Settings.selectedCategories;
    private MainPage mainPage;

    public MainPageSettings(MainPage mainPage)
    {
        InitializeComponent();
        this.mainPage = mainPage;

        categories.Add("IT");
        categories.Add("Creation");
        categories.Add("Sport");
        categories.Add("Science");
        categories.Add("Business");
        categories.Add("Education");

        var collectionView = new CollectionView
        {
            ItemsSource = categories,
            SelectionMode = SelectionMode.None
        };

        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var button = new Button
            {
                TextColor = Colors.Black,
                CornerRadius = 3,
                HeightRequest = 40,
                Padding=-5,
                Margin = new Thickness(0, 5),
                FontAttributes = FontAttributes.Bold,
                BorderColor = Colors.Black,
                BorderWidth = 2,
                FontSize = 18,
            };

            button.SetBinding(Button.TextProperty, ".");

            // Обновляем цвет кнопки в зависимости от состояния
            button.BindingContextChanged += (sender, e) =>
            {
                if (sender is Button btn)
                    if (Settings.selectedCategories.Contains((string)btn.BindingContext))
                        btn.BackgroundColor = Color.FromRgb(61, 61, 61); // Выбранная категория
                    else
                        btn.BackgroundColor = Color.FromRgb(217, 217, 217); // Не выбрана категория
            };

            // Добавляем обработчик нажатия
            button.Clicked += OnCategorySelected;

            return button;
        });


        if (verticalStackLayout != null)
            verticalStackLayout.Children.Insert(1, collectionView); // Индекс 1 — чтобы добавить CollectionView после заголовка
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        // Сохраняем выбранные категории в Settings
        mainPage.FilterCards();
        Close();
    }

    private void OnCategorySelected(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            if (!Settings.selectedCategories.Contains(button.Text))
            {
                button.BackgroundColor = Color.FromRgb(61, 61, 61); // Выбранная категория
                Settings.selectedCategories.Add(button.Text);
            }
            else
            {
                button.BackgroundColor = Color.FromRgb(217, 217, 217); // Не выбрана категория
                Settings.selectedCategories.Remove(button.Text);
            }

            // Логируем текущее состояние после каждого выбора
            Debug.WriteLine("Current selected categories: " + string.Join(", ", Settings.selectedCategories));
        }
    }
}
