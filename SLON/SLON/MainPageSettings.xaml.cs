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
    public HashSet<string> selectedCategories { get; set; } = Settings.selectedCategories;

    public MainPageSettings()
    {
        InitializeComponent();

        categories.Add("IT");
        categories.Add("Creation");
        categories.Add("Sport");
        categories.Add("Science");
        categories.Add("Business");
        categories.Add("Education");

        // Создаём CollectionView программно
        var collectionView = new CollectionView
        {
            ItemsSource = categories,
            SelectionMode = SelectionMode.None
        };

        // Устанавливаем DataTemplate для элементов
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var button = new Button
            {
                TextColor = Colors.Black,
                CornerRadius = 3,
                HeightRequest = 40,
                Margin = new Thickness(0, 5),
                FontAttributes = FontAttributes.Bold,
            };

            // Привязываем текст кнопки
            button.SetBinding(Button.TextProperty, ".");

            // Обновляем цвет кнопки в зависимости от состояния
            button.BindingContextChanged += (sender, e) =>
            {
                if (sender is Button btn)
                {
                    if (selectedCategories.Contains((string)btn.BindingContext))
                    {
                        btn.BackgroundColor = Color.FromRgb(61, 61, 61); // Выбранная категория
                    }
                    else
                    {
                        btn.BackgroundColor = Color.FromRgb(217, 217, 217); // Не выбрана категория
                    }
                }
            };

            // Добавляем обработчик нажатия
            button.Clicked += OnCategorySelected;

            return button;
        });


        if (verticalStackLayout != null)
        {
            verticalStackLayout.Children.Insert(1, collectionView); // Индекс 1 — чтобы добавить CollectionView после заголовка
        }
        else
        {
            Debug.WriteLine("verticalStackLayout не найден.");
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        // Сохраняем выбранные категории в Settings
        Settings.selectedCategories = new HashSet<string>(selectedCategories); // Важно создать новый экземпляр
        Close();
    }

    private void OnCategorySelected(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            // Выбор/отмена выбора категории
            if (!selectedCategories.Contains(button.Text))
            {
                button.BackgroundColor = Color.FromRgb(61, 61, 61); // Выбранная категория
                selectedCategories.Add(button.Text);
            }
            else
            {
                button.BackgroundColor = Color.FromRgb(217, 217, 217); // Не выбрана категория
                selectedCategories.Remove(button.Text);
            }

            // Логируем текущее состояние после каждого выбора
            Debug.WriteLine("Current selected categories: " + string.Join(", ", selectedCategories));
        }
    }
}
