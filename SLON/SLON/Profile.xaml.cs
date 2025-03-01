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
        "IT", "Creation", "Sport", "Science", "Business", "Education", "Social", "Health"
    };

    // Словарь для хранения добавленных категорий и их данных
    private readonly Dictionary<string, (string Tags, string Skills)> addedCategories = new();

    // Список мероприятий
    private readonly List<(string Name, string Categories, string Description, string Location, bool IsOnline)> events = new();

    // Список выбранных категорий
    private readonly List<string> selectedCategories = new();

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
        AddCategoryIcon.IsVisible = SaveIcon.IsVisible = _isEditing;
        EditIcon.IsVisible = !_isEditing;

        if (_isEditing)
        {
            AddEventIcon.Opacity = 0.5;
            AddEventIcon.IsEnabled = false;
        }
        else
        {
            AddEventIcon.Opacity = 1;
            AddEventIcon.IsEnabled = true;
        }

        ToggleDeleteButtons(_isEditing);

        foreach (var frame in EventsContainer.Children.OfType<Frame>())
        {
            frame.GestureRecognizers.Clear();
            if (!_isEditing)
            {
                frame.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => OpenEventPopup(frame.AutomationId, false))
                });
            }
        }

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
        if (!_isEditing) { return; }

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
                deleteButton.IsVisible = false;
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

    private bool _isCreatingEvent = false;

    /// <summary>
    /// Открытие окна создания мероприятия (Независимо от редактирования профиля)
    /// </summary>
    private void OnAddEventIconClicked(object sender, EventArgs e)
    {
        _isCreatingEvent = true;
        _isEditingEvent = false; // Режим создания

        // Сбрасываем данные
        EventNameInput.Text = string.Empty;
        EventDescriptionInput.Text = string.Empty;
        EventLocationInput.Text = string.Empty;
        selectedCategories.Clear();

        foreach (var button in new[] { CategoryIT, CategoryCreation, CategorySport, CategoryScience, CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth })
        {
            button.BackgroundColor = Colors.DarkGray;
            button.BorderColor = Colors.Transparent;
            button.BorderWidth = 0;
        }

        SaveEventButton.Source = "save_icon.png";
        SaveEventButton.IsVisible = true;
        DeleteEventButton.IsVisible = false;

        // Разблокируем поля в режиме создания
        EventNameInput.IsReadOnly = false;
        EventDescriptionInput.IsReadOnly = false;
        EventLocationInput.IsReadOnly = false;

        // Разблокируем кнопки категорий
        EnableCategoryButtons(true);

        EventPopup.IsVisible = true;
    }

    private void EnableCategoryButtons(bool isEnabled)
    {
        foreach (var button in new[] { CategoryIT, CategoryCreation, CategorySport, CategoryScience, CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth })
        {
            button.IsEnabled = isEnabled;
        }
    }

    /// <summary>
    /// Сохранение мероприятия.
    /// </summary>
    private void OnSaveEventClicked(object sender, EventArgs e)
    {
        string name = EventNameInput.Text;
        string categories = string.Join(", ", selectedCategories);
        string description = EventDescriptionInput.Text;
        string location = EventLocationInput.Text;
        bool isOnline = OnlineButton.BackgroundColor == Colors.White;

        if (string.IsNullOrWhiteSpace(name))
        {
            DisplayAlert("Error", "Event name cannot be empty.", "OK");
            return;
        }

        // Добавляем новое мероприятие
        events.Add((name, categories, description, location, isOnline));

        // Обновляем интерфейс и закрываем окно
        RefreshEventsUI();
        EventPopup.IsVisible = false;
    }

    /// <summary>
    /// Обновление интерфейса событий
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
    /// Добавление карточки мероприятия.
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
                new ColumnDefinition { Width = GridLength.Star }
            }
        };

        var label = new Label
        {
            Text = eventName,
            TextColor = Colors.White,
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center
        };

        grid.Children.Add(label);
        Grid.SetColumn(label, 0);

        var deleteButton = new ImageButton
        {
            Source = "trash_icon.png",
            BackgroundColor = Colors.Transparent,
            WidthRequest = 24,
            HeightRequest = 24,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false
        };

        deleteButton.Clicked += (s, e) =>
        {
            events.RemoveAll(ev => ev.Name == eventName);
            RefreshEventsUI();
        };

        grid.Children.Add(deleteButton);
        Grid.SetColumn(deleteButton, 0);

        frame.Content = grid;

        EventsContainer.Children.Add(frame);

        if (!_isEditing)
        {
            frame.GestureRecognizers.Clear();
            frame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OpenEventPopup(eventName, false))
            });
        }
    }

    /// <summary>
    /// Открытие всплывающего окна для просмотра/редактирования мероприятия.
    /// </summary>
    private void OpenEventPopup(string eventName, bool isEditing)
    {
        _isCreatingEvent = string.IsNullOrEmpty(eventName);
        _isEditingEvent = _isCreatingEvent ? false : isEditing;

        var eventData = events.FirstOrDefault(e => e.Name == eventName);

        if (!_isCreatingEvent && eventData.Name == null)
        {
            DisplayAlert("Error", "Event not found.", "OK");
            return;
        }

        // Заполняем поля данными мероприятия (если это не создание нового)
        EventNameInput.Text = _isCreatingEvent ? string.Empty : eventData.Name;
        selectedCategories.Clear();
        if (!_isCreatingEvent)
        {
            selectedCategories.AddRange(eventData.Categories.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries));
        }
        EventDescriptionInput.Text = _isCreatingEvent ? string.Empty : eventData.Description;
        EventLocationInput.Text = _isCreatingEvent ? string.Empty : eventData.Location;

        // Настройка видимости иконок
        SaveEventButton.Source = isEditing ? "save_icon.png" : "edit_icon.png";
        SaveEventButton.IsVisible = true;
        DeleteEventButton.IsVisible = !_isCreatingEvent;

        // Блокируем поля в режиме просмотра
        EventNameInput.IsReadOnly = !isEditing;
        EventDescriptionInput.IsReadOnly = !isEditing;
        EventLocationInput.IsReadOnly = !isEditing;

        EnableCategoryButtons(_isEditingEvent || _isCreatingEvent);

        UpdateCategoryButtons();
        EventPopup.IsVisible = true;
    }

    private void UpdateEventPopupUI()
    {
        SaveEventButton.Source = _isEditingEvent ? "save_icon.png" : "edit_icon.png";
        EventNameInput.IsReadOnly = !_isEditingEvent;
        EventDescriptionInput.IsReadOnly = !_isEditingEvent;
        EventLocationInput.IsReadOnly = !_isEditingEvent;
        EnableCategoryButtons(_isEditingEvent || _isCreatingEvent);
        DeleteEventButton.IsVisible = !_isCreatingEvent;
        UpdateCategoryButtons();
    }

    private void UpdateCategoryButtons()
    {
        foreach (var button in new[] { CategoryIT, CategoryCreation, CategorySport, CategoryScience, CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth })
        {
            if (selectedCategories.Contains(button.Text))
            {
                button.BackgroundColor = GetCategoryColor(button.Text);
                button.BorderColor = Color.FromArgb("#00CED1"); // Бирюзовая обводка
                button.BorderWidth = 3;
            }
            else
            {
                button.BackgroundColor = Colors.DarkGray;
                button.BorderColor = Colors.Transparent;
                button.BorderWidth = 0;
            }
        }
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
    /// Изменения применяются только если мы в режиме редактирования или создания мероприятия.
    /// Цвета кнопок при этом сохраняются во всех режимах.
    /// </summary>
    private void OnLocationButtonClicked(object sender, EventArgs e)
    {
        if (!(_isEditingEvent || _isCreatingEvent))
            return;

        if (sender == OfflineButton)
        {
            OnlineButton.BackgroundColor = Colors.DarkGray;
            OfflineButton.BackgroundColor = Color.FromArgb("#915AC5");
            EventLocationInput.Placeholder = "Venue...";
        }
        else if (sender == OnlineButton)
        {
            OfflineButton.BackgroundColor = Colors.DarkGray;
            OnlineButton.BackgroundColor = Color.FromArgb("#915AC5");
            EventLocationInput.Placeholder = "Link to the meeting...";
        }
    }

    private bool _isEditingEvent = false;

    private void OnEditSaveEventClicked(object sender, EventArgs e)
    {
        if (_isCreatingEvent)
        {
            SaveEventChanges();
            EventPopup.IsVisible = false;
            _isCreatingEvent = false;
        }
        else if (_isEditingEvent)
        {
            SaveEventChanges();
        }
        else { }

        _isEditingEvent = !_isEditingEvent;

        UpdateEventPopupUI();
    }

    private async void OnDeleteEventClicked(object sender, EventArgs e)
    {
        string eventName = EventNameInput.Text;

        bool confirmDelete = await DisplayAlert("Удаление мероприятия", $"Вы уверены, что хотите удалить мероприятие \"{eventName}\"?", "Да", "Нет");

        if (confirmDelete)
        {
            events.RemoveAll(ev => ev.Name == eventName);
            RefreshEventsUI();
            EventPopup.IsVisible = false;
        }
    }

    /// <summary>
    /// Сохраняет изменения мероприятия. При создании нового мероприятия производится проверка на уникальность имени.
    /// </summary>
    private void SaveEventChanges()
    {
        string name = EventNameInput.Text;
        string description = EventDescriptionInput.Text;
        string location = EventLocationInput.Text;
        bool isOnline = OnlineButton.BackgroundColor == Colors.White;

        if (string.IsNullOrWhiteSpace(name))
        {
            DisplayAlert("Error", "Event name cannot be empty.", "OK");
            return;
        }

        string categories = string.Join(", ", selectedCategories);

        if (_isCreatingEvent)
        {
            if (events.Any(ev => ev.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                DisplayAlert("Error", $"Событие с именем \"{name}\" уже существует.", "OK");
                return;
            }
            events.Add((name, categories, description, location, isOnline));
        }
        else
        {
            // обновляем существующее мероприятие
            var existingEvent = events.FirstOrDefault(ev => ev.Name == name);
            if (existingEvent != default)
            {
                events.Remove(existingEvent);
                events.Add((name, categories, description, location, isOnline));
            }
        }

        RefreshEventsUI();
    }

    private void OnCategoryButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            string category = button.Text;

            if (selectedCategories.Contains(category))
            {
                selectedCategories.Remove(category);
                button.BackgroundColor = Colors.DarkGray;
                button.BorderColor = Colors.Transparent;
                button.BorderWidth = 0;
            }
            else
            {
                selectedCategories.Add(category);
                button.BackgroundColor = GetCategoryColor(category);
                button.BorderColor = Color.FromArgb("#00CED1");
                button.BorderWidth = 2;
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
            _ => Colors.DarkGray
        };
    }
}
