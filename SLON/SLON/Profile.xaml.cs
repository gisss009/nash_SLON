using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLON;

public partial class Profile : ContentPage
{
    private bool _isEditing = false;

    // Список категорий
    private readonly List<string> allCategories = new()
    {
        "IT", "Creation", "Sport", "Science", "Business", "Education"
    };

    // Словарь для хранения добавленных категорий и их данных
    private readonly Dictionary<string, (string Tags, string Skills)> addedCategories = new();

    // Список мероприятий
    private readonly List<(string Name, string Tags, string Description, string Location, bool IsOnline)> events = new();

    public Profile()
    {
        InitializeComponent();
        RefreshEventsUI();
    }

    /// <summary>
    /// Переключает режим редактирования для обновления состояния полей и видимости элементов интерфейса.
    /// </summary>
    private void OnEditIconClicked(object sender, EventArgs e)
    {
        _isEditing = !_isEditing;

        // Переключаем состояние редактирования элементов
        NameInput.IsReadOnly = ResumeEditor.IsReadOnly = VocationInput.IsReadOnly = !_isEditing;
        AddCategoryIcon.IsVisible = AddEventIcon.IsVisible = SaveIcon.IsVisible = _isEditing;
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
    /// Открытие окна для добавления новой категории.
    /// </summary>
    private async void OnAddCategoryIconClicked(object sender, EventArgs e)
    {
        string selectedCategory = await DisplayActionSheet("Выберите категорию", "Отмена", null, allCategories.Except(addedCategories.Keys).ToArray());

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
        if (!_isEditing) {  return; }
            
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

        categoryFrame.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => OpenCategoryPopup(categoryName)) });
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

        foreach (var frame in EventsContainer.Children.OfType<Frame>())
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

    /// <summary>
    /// Открытие окна создания мероприятия.
    /// </summary>
    private void OnAddEventIconClicked(object sender, EventArgs e)
    {
        EventNameInput.Text = string.Empty;
        EventTagsInput.Text = string.Empty;
        EventDescriptionInput.Text = string.Empty;
        EventLocationInput.Text = string.Empty;

        EventPopup.IsVisible = true;
    }

    /// <summary>
    /// Сохранение мероприятия.
    /// </summary>
    private void OnSaveEventClicked(object sender, EventArgs e)
    {
        string name = EventNameInput.Text;
        string tags = EventTagsInput.Text;
        string description = EventDescriptionInput.Text;
        string location = EventLocationInput.Text;
        bool isOnline = OnlineButton.BackgroundColor == Colors.White;

        if (string.IsNullOrWhiteSpace(name))
        {
            DisplayAlert("Error", "Event name cannot be empty.", "OK");
            return;
        }

        var existingEvent = events.FirstOrDefault(ev => ev.Name == name);
        if (!string.IsNullOrEmpty(existingEvent.Name))
        {
            events.Remove(existingEvent);
        }

        events.Add((name, tags, description, location, isOnline));
        RefreshEventsUI();

        EventPopup.IsVisible = false;
    }

    /// <summary>
    /// Перерисовка всех карточек мероприятий
    /// </summary>
    private void RefreshEventsUI()
    {
        EventsContainer.Children.Clear();

        foreach (var ev in events)
        {
            AddEventCard(ev.Name);
        }
    }

    /// <summary>
    /// Добавляет карточку мероприятия
    /// </summary>
    private void AddEventCard(string eventName)
    {
        var frame = new Frame
        {
            AutomationId = eventName,
            BackgroundColor = Colors.Gray,
            CornerRadius = 5,
            Padding = 10,
            Margin = 5
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
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
            IsVisible = _isEditing
        };

        deleteButton.Clicked += (s, e) =>
        {
            EventsContainer.Children.Remove(frame);
            events.RemoveAll(ev => ev.Name == eventName);
        };

        var label = new Label
        {
            Text = eventName,
            TextColor = Colors.White,
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center
        };

        grid.Children.Add(deleteButton);
        Grid.SetColumn(deleteButton, 0);
        grid.Children.Add(label);
        Grid.SetColumn(label, 1);

        frame.Content = grid;

        EventsContainer.Children.Add(frame);

        frame.GestureRecognizers.Clear();
        frame.GestureRecognizers.Add(new TapGestureRecognizer{Command = new Command(() =>{OpenEventPopup(eventName, _isEditing);})});
    }

    /// <summary>
    /// Открытие всплывающего окна для просмотра/редактирования мероприятия.
    /// </summary>
    private void OpenEventPopup(string eventName, bool isEditing)
    {
        var eventData = events.FirstOrDefault(e => e.Name == eventName);

        EventNameInput.Text = eventData.Name;
        EventTagsInput.Text = eventData.Tags;
        EventDescriptionInput.Text = eventData.Description;
        EventLocationInput.Text = eventData.Location;

        EventNameInput.IsReadOnly = EventTagsInput.IsReadOnly = EventDescriptionInput.IsReadOnly = EventLocationInput.IsReadOnly = !isEditing;
        SaveEventButton.IsVisible = OfflineButton.IsEnabled = OnlineButton.IsEnabled = isEditing;

        EventPopup.IsVisible = true;
    }

    /// <summary>
    /// Закрытие всплывающего окна мероприятия.
    /// </summary>
    private void OnCancelEventClicked(object sender, EventArgs e)
    {
        EventPopup.IsVisible = false;
    }

    /// <summary>
    /// Переключение состояния кнопки Online/Offline.
    /// </summary>
    private void OnLocationButtonClicked(object sender, EventArgs e)
    {
        if (sender == OnlineButton)
        {
            OnlineButton.BackgroundColor = Colors.DarkGray;
            OfflineButton.BackgroundColor = Colors.Black;

            EventLocationInput.Placeholder = "Venue...";
        }
        else if (sender == OfflineButton)
        {
            OfflineButton.BackgroundColor = Colors.DarkGray;
            OnlineButton.BackgroundColor = Colors.Black;

            EventLocationInput.Placeholder = "Link to the meeting...";
        }
    }
}

