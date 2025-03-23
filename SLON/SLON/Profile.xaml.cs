using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLON
{
    public partial class Profile : ContentPage
    {
        private bool _isEditing = false;          // Режим редактирования профиля
        private bool _isEditingEvent = false;     // Режим редактирования события
        private bool _isCreatingEvent = false;    // Режим создания нового события

        // Переключатель In/My: true – мои события, false – события, где я участвую
        private bool _showMyEvents = false;

        // Списки категорий и событий
        private readonly List<string> allCategories = new() { "IT", "Creation", "Sport", "Science", "Business", "Education", "Social", "Health" };
        private readonly Dictionary<string, (string Tags, string Skills)> addedCategories = new();
        // События: (Name, Categories, Description, Location, IsOnline, IsPublic, StartDate, EndDate, IsMyEvent)
        private readonly List<(string Name, string Categories, string Description, string Location,
                               bool IsOnline, bool IsPublic, DateTime StartDate, DateTime EndDate, bool IsMyEvent)> events = new();
        private readonly List<string> selectedCategories = new();

        private bool _isPublic = true;
        private bool _isOnline = false;
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today;
        private string _originalEventName = string.Empty;

        public Profile()
        {
            InitializeComponent();

            StartDatePicker.MinimumDate = DateTime.Today;
            EndDatePicker.MinimumDate = DateTime.Today;

            events.Add(("InEvent1", "Science", "Event I'm in", "Venue D", false, true, DateTime.Today, DateTime.Today.AddDays(5), false));
            events.Add(("InEvent2", "Business", "Another event I'm in", "Venue E", false, true, DateTime.Today, DateTime.Today.AddDays(6), false));
            events.Add(("InEvent3", "Education", "Yet another event I'm in", "Venue F", false, false, DateTime.Today, DateTime.Today.AddDays(7), false));
            events.Add(("InEvent4", "Education, Business", "Yet another event I'm in", "Venue F", false, false, DateTime.Today, DateTime.Today.AddDays(7), false));
            RefreshEventsUI();
        }

        #region Редактирование профиля

        private void OnEditIconClicked(object sender, EventArgs e)
        {
            _isEditing = !_isEditing;
            SetProfileEditMode(_isEditing);
            ToggleEventCardTaps(!_isEditing);
            if (!_isEditing) SaveProfileChanges();
        }

        private void SetProfileEditMode(bool isEditing)
        {
            NameInput.IsReadOnly = SurnameInput.IsReadOnly = VocationInput.IsReadOnly = ResumeEditor.IsReadOnly = !isEditing;
            AddCategoryIcon.IsVisible = SaveIcon.IsVisible = isEditing;
            EditIcon.IsVisible = !isEditing;
            AddEventIcon.Opacity = isEditing ? 0.5 : 1;
            AddEventIcon.IsEnabled = !isEditing;
            ToggleDeleteButtons(isEditing);
            RefreshCategoriesUI();
        }

        private void ToggleEventCardTaps(bool enable)
        {
            foreach (var frame in EventsContainer.Children.OfType<Frame>())
            {
                frame.GestureRecognizers.Clear();
                if (enable)
                {
                    frame.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(() => OpenEventPopup(frame.AutomationId, false))
                    });
                }
            }
        }

        private void SaveProfileChanges()
        {
            DisplayAlert("Success", "Profile changes saved!", "OK");
        }

        #endregion

        #region Работа с аватаром

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

        #endregion

        #region Управление категориями

        private async void OnAddCategoryIconClicked(object sender, EventArgs e)
        {
            string[] availableCategories = allCategories.Except(addedCategories.Keys).ToArray();
            if (availableCategories.Length == 0)
            {
                await DisplayAlert("Info", "All categories are already added.", "OK");
                return;
            }
            string selectedCategory = await DisplayActionSheet("Выберите категорию", "Отмена", null, availableCategories);
            if (!string.IsNullOrWhiteSpace(selectedCategory) && selectedCategory != "Отмена")
                OpenCategoryPopup(selectedCategory);
        }

        private void OpenCategoryPopup(string categoryName)
        {
            if (!_isEditing) return;
            CategoryPopup.IsVisible = true;
            CategoryNameLabel.Text = categoryName;
            if (addedCategories.TryGetValue(categoryName, out var data))
            {
                TagsEditor.Text = data.Tags;
                SkillsEditor.Text = data.Skills;
            }
            else
            {
                TagsEditor.Text = SkillsEditor.Text = string.Empty;
            }
        }

        private void OnSaveCategoryClicked(object sender, EventArgs e)
        {
            string categoryName = CategoryNameLabel.Text;
            addedCategories[categoryName] = (TagsEditor.Text, SkillsEditor.Text);

            RefreshCategoriesUI();

            CategoryPopup.IsVisible = false;
        }       

        private void ToggleDeleteButtons(bool isVisible)
        {
            foreach (var frame in CategoriesGrid.Children.OfType<Frame>())
            {
                if ((frame.Content as Grid)?.Children.OfType<ImageButton>().FirstOrDefault() is ImageButton deleteButton)
                    deleteButton.IsVisible = isVisible;
            }
        }

        #endregion

        #region Управление событиями

        private void OnAddEventIconClicked(object sender, EventArgs e)
        {
            if (_isEditing) return;
            _isCreatingEvent = true;
            _isEditingEvent = false;
            ResetEventPopup();
            EventPopup.IsVisible = true;
        }

        private void ResetEventPopup()
        {
            EventNameInput.Text = string.Empty;
            EventDescriptionInput.Text = string.Empty;
            EventLocationInput.Text = string.Empty;
            selectedCategories.Clear();

            _startDate = _endDate = DateTime.Today;
            StartDatePicker.Date = _startDate;
            EndDatePicker.Date = _endDate;

            _isPublic = true;
            _isOnline = false;

            PublicButton.BackgroundColor = Color.FromArgb("#915AC5");
            PrivateButton.BackgroundColor = Colors.DarkGray;
            OnlineButton.BackgroundColor = Colors.DarkGray;
            OfflineButton.BackgroundColor = Color.FromArgb("#915AC5");

            EventLocationInput.Placeholder = "Venue...";
            ResetCategoryButtons();
            SaveEventButton.Source = "save_icon.png";

            SaveEventButton.IsVisible = true;
            DeleteEventButton.IsVisible = false;
            EventNameInput.IsReadOnly = false;
            EventDescriptionInput.IsReadOnly = false;
            EventLocationInput.IsReadOnly = false;
            EnableCategoryButtons(true);
            StartDatePicker.IsEnabled = true;
            EndDatePicker.IsEnabled = true;
        }

        private void ResetCategoryButtons()
        {
            foreach (var button in new[]{CategoryIT, CategoryCreation, CategorySport, CategoryScience,CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth})
            {
                button.BackgroundColor = Colors.DarkGray;
                button.BorderColor = Colors.Transparent;
                button.BorderWidth = 0;
            }
        }

        private void EnableCategoryButtons(bool isEnabled)
        {
            foreach (var button in new[]{CategoryIT, CategoryCreation, CategorySport, CategoryScience,CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth})
                button.IsEnabled = isEnabled;
        }

        private void OnEditSaveEventClicked(object sender, EventArgs e)
        {
            if (_isCreatingEvent || _isEditingEvent)
            {
                SaveEventChanges();
                if (_isCreatingEvent)
                {
                    _isCreatingEvent = false;
                    EventPopup.IsVisible = false;
                    return;
                }
            }

            if (IsCurrentEventMine())
            {
                // Переключаем режим редактирования
                _isEditingEvent = !_isEditingEvent;
                // Обновляем иконку в зависимости от текущего режима
                SaveEventButton.Source = _isEditingEvent ? "save_icon.png" : "edit_icon.png";
                UpdateEventPopupUI();
            }
        }

        private void RefreshCategoriesUI()
        {
            CategoriesGrid.Children.Clear();
            CategoriesGrid.RowDefinitions.Clear();

            var catList = addedCategories.Keys.ToList();
            int itemsPerRow = 3;
            int totalCategories = catList.Count;
            int rows = (int)Math.Ceiling((double)totalCategories / itemsPerRow);

            for (int r = 0; r < rows; r++)
            {
                CategoriesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < itemsPerRow; c++)
                {
                    if (index >= totalCategories)
                        break;

                    string categoryName = catList[index];
                    var chip = CreateCategoryChip(categoryName);
                    CategoriesGrid.Add(chip, c, r);

                    index++;
                }
            }
        }

        private View CreateCategoryChip(string categoryName)
        {
            var frame = new Frame
            {
                AutomationId = categoryName,
                CornerRadius = 15,
                Padding = new Thickness(10, 5),
                Margin = new Thickness(5),
                HasShadow = false,
                BackgroundColor = GetCategoryColor(categoryName),
                WidthRequest = 107, // размеры чипов
                HeightRequest = 40
            };

            var grid = new Grid{ ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star }}};

            var deleteButton = new ImageButton
            {
                Source = "delete_icon.png",
                BackgroundColor = Colors.Transparent,
                WidthRequest = 16,
                HeightRequest = 16,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = _isEditing // кнопка видна только в режиме редактирования
            };
            deleteButton.Clicked += (s, e) =>
            {
                addedCategories.Remove(categoryName);
                RefreshCategoriesUI();
            };

            var categoryLabel = new Label{Text = categoryName,TextColor = Colors.White,FontSize = 14,VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };

            grid.Children.Add(deleteButton);
            Grid.SetColumn(deleteButton, 0);
            grid.Children.Add(categoryLabel);
            Grid.SetColumn(categoryLabel, 1);

            frame.Content = grid;

            // Если хотим, чтобы по тапу открывалось окно редактирования Tags/Skills
            frame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OpenCategoryPopup(categoryName))
            });

            return frame;
        }


        private void OnDeleteEventClicked(object sender, EventArgs e)
        {
            string eventName = EventNameInput.Text;
            Dispatcher.Dispatch(async () =>
            {
                bool confirm = await DisplayAlert("Удаление мероприятия",
                    $"Вы уверены, что хотите удалить мероприятие \"{eventName}\"?", "Да", "Нет");
                if (confirm)
                {
                    events.RemoveAll(ev => ev.Name == eventName);
                    RefreshEventsUI();
                    EventPopup.IsVisible = false;
                }
            });
        }

        private void SaveEventChanges()
        {
            string name = EventNameInput.Text;

            // Проверка на длину
            if (!string.IsNullOrWhiteSpace(name) && name.Length > 20)
            {
                DisplayAlert("Error", "Event name is too long (max 20 characters).", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                DisplayAlert("Error", "Event name cannot be empty.", "OK");
                return;
            }

            // Дальше - ваша логика дат
            if (_endDate < _startDate)
            {
                DisplayAlert("Ошибка", "Дата окончания не может быть раньше даты начала.", "OK");
                return;
            }

            string description = EventDescriptionInput.Text;
            string location = EventLocationInput.Text;
            string categories = string.Join(", ", selectedCategories);
            bool isOnline = _isOnline;

            if (_isCreatingEvent)
            {
                if (events.Any(ev => ev.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    DisplayAlert("Error", $"Событие с именем \"{name}\" уже существует.", "OK");
                    return;
                }
                events.Add((name, categories, description, location, isOnline, _isPublic, _startDate, _endDate, true));
            }
            else
            {
                var existingEvent = events.FirstOrDefault(ev => ev.Name == _originalEventName);
                if (existingEvent != default)
                {
                    bool wasMyEvent = existingEvent.IsMyEvent;
                    events.Remove(existingEvent);
                    events.Add((name, categories, description, location, isOnline, _isPublic, _startDate, _endDate, wasMyEvent));
                    _originalEventName = name;
                }
            }

            RefreshEventsUI();

            if (AllEventsPopup.IsVisible)
            {
                PopulateAllEventsPopup();
            }
        }


        private void RefreshEventsUI()
        {
            EventsContainer.Children.Clear();
            // Фильтрация по переключателю In/My
            var filtered = _showMyEvents ? events.Where(ev => ev.IsMyEvent).ToList() : events.Where(ev => !ev.IsMyEvent).ToList();
            var displayList = filtered.Take(3).ToList();
            foreach (var ev in displayList)
                AddEventCard(ev.Name, ev.IsMyEvent);
            ShowAllEventsButton.IsVisible = true;
        }

        private void AddEventCard(string eventName, bool isMyEvent)
        {
            var frame = new Frame{AutomationId = eventName,BackgroundColor = Color.FromArgb("#353535"),CornerRadius = 10,Padding = 10,Margin = 5,HeightRequest = 40};
            var grid = new Grid();
            var label = new Label{Text = eventName,TextColor = Colors.White,FontSize = 16,VerticalOptions = LayoutOptions.Center};
            grid.Children.Add(label);
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

        // Открывает окно события
        private void OpenEventPopup(string eventName, bool isEditing)
        {
            _isCreatingEvent = string.IsNullOrEmpty(eventName);
            var eventData = events.FirstOrDefault(e => e.Name == eventName);
            if (!_isCreatingEvent && string.IsNullOrEmpty(eventData.Name))
            {
                DisplayAlert("Error", "Event not found.", "OK");
                return;
            }
            _originalEventName = _isCreatingEvent ? "" : eventData.Name;
            EventNameInput.Text = _isCreatingEvent ? "" : eventData.Name;
            selectedCategories.Clear();
            if (!_isCreatingEvent)
                selectedCategories.AddRange(eventData.Categories.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries));

            EventDescriptionInput.Text = _isCreatingEvent ? "" : eventData.Description;
            EventLocationInput.Text = _isCreatingEvent ? "" : eventData.Location;

            _startDate = _isCreatingEvent ? DateTime.Today : eventData.StartDate;
            _endDate = _isCreatingEvent ? DateTime.Today : eventData.EndDate;
            StartDatePicker.Date = _startDate;
            EndDatePicker.Date = _endDate;

            bool canEditDates = _isEditingEvent || _isCreatingEvent;
            StartDatePicker.IsEnabled = EndDatePicker.IsEnabled = canEditDates;

            _isPublic = _isCreatingEvent ? true : eventData.IsPublic;
            PublicButton.BackgroundColor = _isPublic ? Color.FromArgb("#915AC5") : Colors.DarkGray;
            PrivateButton.BackgroundColor = _isPublic ? Colors.DarkGray : Color.FromArgb("#915AC5");
            _isOnline = _isCreatingEvent ? false : eventData.IsOnline;

            if (_isOnline)
            {
                OnlineButton.BackgroundColor = Color.FromArgb("#915AC5");
                OfflineButton.BackgroundColor = Colors.DarkGray;
                EventLocationInput.Placeholder = "Link to the meeting...";
            }
            else
            {
                OnlineButton.BackgroundColor = Colors.DarkGray;
                OfflineButton.BackgroundColor = Color.FromArgb("#915AC5");
                EventLocationInput.Placeholder = "Venue...";
            }
            if (!eventData.IsMyEvent)
            {
                SaveEventButton.IsVisible = false;
                DeleteEventButton.IsVisible = false;
                EventNameInput.IsReadOnly = true;
                EventDescriptionInput.IsReadOnly = true;
                EventLocationInput.IsReadOnly = true;
                EnableCategoryButtons(false);
                StartDatePicker.IsEnabled = false;
                EndDatePicker.IsEnabled = false;
            }
            else
            {
                SaveEventButton.IsVisible = true;
                DeleteEventButton.IsVisible = !_isCreatingEvent;
                // Сброс режима редактирования при открытии собственного мероприятия:
                _isEditingEvent = false;
                SaveEventButton.Source = "edit_icon.png";
                UpdateEventPopupUI();
            }
            UpdateCategoryButtons();
            EventPopup.IsVisible = true;
        }


        private bool IsCurrentEventMine()
        {
            var ev = events.FirstOrDefault(e => e.Name == _originalEventName);
            return ev != default && ev.IsMyEvent;
        }

        private void OnStartDateSelected(object sender, DateChangedEventArgs e)
        {
            _startDate = e.NewDate.Date;
            if (_endDate < _startDate)
            {
                _endDate = _startDate;
                EndDatePicker.Date = _endDate;
            }
        }

        private void OnEndDateSelected(object sender, DateChangedEventArgs e)
        {
            _endDate = e.NewDate.Date;
            if (_endDate < _startDate)
            {
                DisplayAlert("Ошибка", "Дата окончания не может быть раньше даты начала.", "OK");
                EndDatePicker.Date = _startDate;
            }
        }

        private void UpdateEventPopupUI()
        {
            SaveEventButton.Source = _isEditingEvent ? "save_icon.png" : "edit_icon.png";
            EventNameInput.IsReadOnly = !_isEditingEvent;
            EventDescriptionInput.IsReadOnly = !_isEditingEvent;
            EventLocationInput.IsReadOnly = !_isEditingEvent;
            bool canEditDates = _isEditingEvent || _isCreatingEvent;
            StartDatePicker.IsEnabled = EndDatePicker.IsEnabled = canEditDates;
            EnableCategoryButtons(canEditDates);
            UpdateCategoryButtons();
        }

        private void OnLocationButtonClicked(object sender, EventArgs e)
        {
            if (!(_isEditingEvent || _isCreatingEvent)) return;
            if (sender == OfflineButton)
            {
                _isOnline = false;
                OnlineButton.BackgroundColor = Colors.DarkGray;
                OfflineButton.BackgroundColor = Color.FromArgb("#915AC5");
                EventLocationInput.Placeholder = "Venue...";
            }
            else if (sender == OnlineButton)
            {
                _isOnline = true;
                OfflineButton.BackgroundColor = Colors.DarkGray;
                OnlineButton.BackgroundColor = Color.FromArgb("#915AC5");
                EventLocationInput.Placeholder = "Link to the meeting...";
            }
        }

        private void OnPublicPrivateButtonClicked(object sender, EventArgs e)
        {
            if (!(_isEditingEvent || _isCreatingEvent)) return;
            if (sender == PublicButton)
            {
                _isPublic = true;
                PublicButton.BackgroundColor = Color.FromArgb("#915AC5");
                PrivateButton.BackgroundColor = Colors.DarkGray;
            }
            else if (sender == PrivateButton)
            {
                _isPublic = false;
                PrivateButton.BackgroundColor = Color.FromArgb("#915AC5");
                PublicButton.BackgroundColor = Colors.DarkGray;
            }
        }

        private void OnCancelEventClicked(object sender, EventArgs e)
        {
            EventPopup.IsVisible = false;
        }

        #endregion

        #region Категории для события

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

        private void UpdateCategoryButtons()
        {
            foreach (var button in new[]
            {
                CategoryIT, CategoryCreation, CategorySport, CategoryScience,
                CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth
            })
            {
                if (selectedCategories.Contains(button.Text))
                {
                    button.BackgroundColor = GetCategoryColor(button.Text);
                    button.BorderColor = Color.FromArgb("#00CED1");
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
                _ => Colors.DarkGray,
            };
        }

        #endregion

        #region Переключатель In/My и Show all

        private void OnInMyEventsButtonClicked(object sender, EventArgs e)
        {
            if (sender == InEventsButton)
            {
                _showMyEvents = false;
                InEventsButton.BackgroundColor = Color.FromArgb("#915AC5");
                MyEventsButton.BackgroundColor = Colors.DarkGray;
            }
            else if (sender == MyEventsButton)
            {
                _showMyEvents = true;
                MyEventsButton.BackgroundColor = Color.FromArgb("#915AC5");
                InEventsButton.BackgroundColor = Colors.DarkGray;
            }
            RefreshEventsUI();
        }

        // Кнопка Show all всегда открывает окно со всеми событиями (сортировка по дате – новейшие сверху)
        private void OnShowAllEventsClicked(object sender, EventArgs e)
        {
            PopulateAllEventsPopup();
            AllEventsPopup.IsVisible = true;
        }

        private void PopulateAllEventsPopup()
        {
            AllEventsContainer.Children.Clear();
            var filtered = _showMyEvents ? events.Where(ev => ev.IsMyEvent).ToList() : events.Where(ev => !ev.IsMyEvent).ToList();
            var sorted = filtered.OrderByDescending(ev => ev.StartDate).ToList();
            foreach (var ev in sorted)
            {
                var frame = new Frame
                {
                    AutomationId = ev.Name,
                    BackgroundColor = Color.FromArgb("#353535"),
                    CornerRadius = 10,
                    Padding = 10,
                    Margin = 5,
                    HeightRequest = 40
                };
                var grid = new Grid();
                var label = new Label
                {
                    Text = ev.Name,
                    TextColor = Colors.White,
                    FontSize = 16,
                    VerticalOptions = LayoutOptions.Center
                };
                grid.Children.Add(label);
                frame.Content = grid;
                AllEventsContainer.Children.Add(frame);
                frame.GestureRecognizers.Clear();
                frame.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        if (ev.IsMyEvent)
                            OpenEventPopup(ev.Name, true);
                        else
                            OpenEventPopup(ev.Name, false);
                        // Не закрываем автоматически окно Show all – его можно закрыть по кнопке
                    })
                });
            }
        }

        private void OnCloseAllEventsClicked(object sender, EventArgs e)
        {
            AllEventsPopup.IsVisible = false;
        }

        #endregion
    }
}
