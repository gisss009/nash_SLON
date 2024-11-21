using System.Globalization;

namespace SLON;

public partial class Profile : ContentPage
{
    private bool _isEditing = false;

    // Список категорий
    private readonly List<string> allCategories = new()
    {
        "IT", "Creation", "Sport", "Science", "Business", "Education"
    };

    // Список для хранения добавленных категорий и их данных
    private readonly Dictionary<string, (string Tags, string Skills)> addedCategories = new();

    public Profile()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Переключает режим редактирования для обновления состояния полей и видимости элементов интерфейса.
    /// </summary>
    private void OnEditIconClicked(object sender, EventArgs e)
    {
        _isEditing = !_isEditing;

        // Переключаем состояние редактирования элементов
        NameInput.IsReadOnly = ResumeEditor.IsReadOnly = VocationInput.IsReadOnly = !_isEditing;
        AddCategoryIcon.IsVisible = SaveIcon.IsVisible = _isEditing;
        EditIcon.IsVisible = !_isEditing;

        // Переключаем видимость кнопок удаления у карточек
        ToggleDeleteButtons(_isEditing);

        if (!_isEditing)
        {
            SaveProfileChanges();
        }
    }

    /// <summary>
    /// Обновляет аватар пользователя.
    /// </summary>
    private async void OnAvatarButtonClicked(object sender, EventArgs e)
    {
        if (!_isEditing) return;

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите изображение:",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                AvatarButton.Source = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить изображение: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Открытие окна для добавления новой категории
    /// </summary>
    private async void OnAddCategoryIconClicked(object sender, EventArgs e)
    {
        string selectedCategory = await DisplayActionSheet("Выберите категорию","Отмена",null,allCategories.Except(addedCategories.Keys).ToArray());

        if (string.IsNullOrWhiteSpace(selectedCategory) || selectedCategory == "Отмена")
        {
            return;
        }

        OpenCategoryPopup(selectedCategory);
    }

    /// <summary>
    /// Открытие всплывающего окна для редактирования/добавления категории.
    /// </summary>
    private void OpenCategoryPopup(string categoryName)
    {
        CategoryPopup.IsVisible = true;
        CategoryNameLabel.Text = categoryName;

        if (addedCategories.TryGetValue(categoryName, out var data))
        {
            TagsEditor.Text = data.Tags;
            SkillsEditor.Text = data.Skills;
        }
        else
        {
            TagsEditor.Text = string.Empty;
            SkillsEditor.Text = string.Empty;
        }
    }

    /// <summary>
    /// Сохраняет изменения в категории.
    /// </summary>
    private void OnSaveCategoryClicked(object sender, EventArgs e)
    {
        string categoryName = CategoryNameLabel.Text;
        string tags = TagsEditor.Text;
        string skills = SkillsEditor.Text;

        addedCategories[categoryName] = (tags, skills);

        UpdateCategoryCard(categoryName);

        CategoryPopup.IsVisible = false;
    }

    /// <summary>
    /// Обновление существующей карточки категории или добавление новой.
    /// </summary>
    private void UpdateCategoryCard(string categoryName)
    {
        // Проверка на существование карточки для данной категории
        var existingCard = CategoriesContainer.Children.OfType<Frame>().FirstOrDefault(f => f.AutomationId == categoryName);

        if (existingCard != null)
        {
            var grid = existingCard.Content as Grid;
            var label = grid?.Children.OfType<Label>().FirstOrDefault();
            if (label != null)
            {
                label.Text = categoryName;
            }
            return;
        }

        var categoryFrame = new Frame
        {
            AutomationId = categoryName,
            BackgroundColor = Colors.Gray,
            CornerRadius = 5,
            Padding = new Thickness(10),
            Margin = new Thickness(5),
            HasShadow = false
        };

        var categoryGrid = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star }
        }
        };

        var deleteButton = new ImageButton
        {
            Source = "delete_icon.png",
            BackgroundColor = Colors.Transparent,
            WidthRequest = 16,
            HeightRequest = 16,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = _isEditing
        };

        deleteButton.Clicked += (s, e) =>
        {
            CategoriesContainer.Children.Remove(categoryFrame);
            addedCategories.Remove(categoryName);
        };

        var categoryLabel = new Label
        {
            Text = categoryName,
            TextColor = Colors.White,
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };

        categoryGrid.Children.Add(deleteButton);
        Grid.SetColumn(deleteButton, 0);
        categoryGrid.Children.Add(categoryLabel);
        Grid.SetColumn(categoryLabel, 1);

        categoryFrame.Content = categoryGrid;

        CategoriesContainer.Children.Add(categoryFrame);

        categoryFrame.GestureRecognizers.Add(new TapGestureRecognizer{Command = new Command(() => OpenCategoryPopup(categoryName))});
    }


    /// <summary>
    /// Переключает видимость кнопок удаления для карточек.
    /// </summary>
    private void ToggleDeleteButtons(bool isVisible)
    {
        foreach (var frame in CategoriesContainer.Children.OfType<Frame>())
        {
            if (frame.Content is Grid grid && grid.Children.OfType<ImageButton>().FirstOrDefault() is ImageButton deleteButton)
            {
                deleteButton.IsVisible = isVisible;
            }
        }
    }

    /// <summary>
    /// Сохранение изменений профиля.
    /// </summary>
    private void SaveProfileChanges()
    {
        DisplayAlert("Success", "Profile changes saved!", "OK");
    }
}
