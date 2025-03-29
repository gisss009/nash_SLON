using Microsoft.Maui.Controls;
using SLON.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SLON
{
    public partial class Profile : ContentPage
    {
        private bool _isEditing = false;          // Режим редактирования профиля
        private bool _isEditingEvent = false;     // Режим редактирования события
        private bool _isCreatingEvent = false;    // Режим создания нового события

        // Переключатель In/My: true – мои события, false – события, где я участвую
        private bool _showMyEvents = true;

        // Списки категорий и событий
        private readonly List<string> allCategories = new() { "IT", "Creation", "Sport", "Science", "Business", "Education", "Social", "Health" };
        private readonly Dictionary<string, (string Tags, string Skills)> addedCategories = new();
        // События: (Hash, Name, Categories, Description, Location, IsOnline, IsPublic, StartDate, EndDate, IsMyEvent)
        private readonly List<(string Hash, string Name, string Categories, string Description, string Location,
                               bool IsOnline, bool IsPublic, DateTime StartDate, DateTime EndDate, bool IsMyEvent)> events = new();
        private readonly List<string> selectedCategories = new();

        private bool _isPublic = true;
        private bool _isOnline = false;
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today;
        private string _originalEventHash = string.Empty; // Хранит хеш редактируемого события

        private readonly Dictionary<string, (string TagExample, string SkillExample)> categoryExamples = new() {
            {"IT", ("C# Python DevOps", "Backend Development, Cloud Architecture")},
            {"Creation", ("Photography Illustration Typography", "Graphic Design, 3D Modeling")},
            {"Sport", ("Yoga Crossfit Marathon", "Team Coaching, Personal Training")},
            {"Science", ("Biology Chemistry Physics", "Lab Research, Data Analysis")},
            {"Business", ("Marketing Finance Startup", "Project Management, Investments")},
            {"Education", ("Pedagogy STEM E-learning", "Curriculum Development, Tutoring")},
            {"Social", ("Volunteering NGO EventPlanning", "Community Management, Public Speaking")},
            {"Health", ("Nutrition Therapy Fitness", "Diet Planning, Rehabilitation")}
        };

        private AuthService.UserProfile _currentProfile;
        public Profile()
        {
            InitializeComponent();

            StartDatePicker.MinimumDate = DateTime.Today;
            EndDatePicker.MinimumDate = DateTime.Today;
            ResumeEditor.Placeholder = "Description is empty";

            _showMyEvents = true;
            MyEventsButton.BackgroundColor = Color.FromArgb("#915AC5");
            InEventsButton.BackgroundColor = Colors.DarkGray;

            events.Add((Guid.NewGuid().ToString(), "InEvent1", "Science", "Event I'm in", "Venue D", false, true, DateTime.Today, DateTime.Today.AddDays(5), false));
            events.Add((Guid.NewGuid().ToString(), "InEvent2", "Business", "Another event I'm in", "Venue E", false, true, DateTime.Today, DateTime.Today.AddDays(6), false));
            events.Add((Guid.NewGuid().ToString(), "InEvent3", "Education", "Yet another event I'm in", "Venue F", false, false, DateTime.Today, DateTime.Today.AddDays(7), false));
            events.Add((Guid.NewGuid().ToString(), "InEvent4", "Education, Business", "Yet another event I'm in", "Venue F", false, false, DateTime.Today, DateTime.Today.AddDays(7), false));
            events.Add((Guid.NewGuid().ToString(), "InEvent4", "Education, Business", "Yet another event I'm in", "Venue F", false, false, DateTime.Today, DateTime.Today.AddDays(7), true));
            events.Add((Guid.NewGuid().ToString(), "In", "Education, Business", "Yet another event I'm in", "Venue F", false, false, DateTime.Today, DateTime.Today.AddDays(7), true));
            RefreshEventsUI();
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            try
            {
                var username = AuthService.GetUsernameAsync();
                _currentProfile = await AuthService.GetUserProfileAsync(username);

                if (_currentProfile == null)
                {
                    await DisplayAlert("Ошибка", "Не удалось загрузить профиль", "OK");
                    return;
                }

                // Инициализируем оригинальные данные
                _originalProfileData = new UserProfileEditModel
                {
                    Name = _currentProfile.name ?? "",
                    Surname = _currentProfile.surname ?? "",
                    Vocation = _currentProfile.vocation ?? "",
                    Description = _currentProfile.description ?? "",
                    Categories = _currentProfile.categories ?? new List<string>()
                };

                UpdateProfileUI(_currentProfile);
                await LoadAndRefreshEvents(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex}");
                await DisplayAlert("Ошибка", "Произошла ошибка при загрузке данных", "OK");
            }
        }

        private async Task LoadAndRefreshEvents(string username)
        {
            try
            {
                List<AuthService.EventData> got_events = await AuthService.GetAllUserEventsAsync(username);

                events.Clear();
                foreach (var eventData in got_events)
                {
                    events.Add((
                        eventData.hash,
                        eventData.name,
                        string.Join(", ", eventData.categories),
                        eventData.description,
                        eventData.location,
                        eventData.online == 1,
                        eventData.@public == 1,
                        DateTime.ParseExact(eventData.date_from, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        DateTime.ParseExact(eventData.date_to, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        eventData.owner == username
                    ));
                }

                RefreshEventsUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке мероприятий: {ex}");
                await DisplayAlert("Ошибка", "Не удалось загрузить мероприятия", "OK");
            }
        }

        // Модель для хранения оригинальных данных профиля
        private class UserProfileEditModel
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Vocation { get; set; }
            public string Description { get; set; }
            public List<string> Categories { get; set; }
        }
        private UserProfileEditModel _originalProfileData;

        private void UpdateProfileUI(AuthService.UserProfile profile)
        {
            UsernameLabel.Text = "@" + (profile?.username ?? "");
            NameInput.Text = profile?.name ?? "";
            SurnameInput.Text = profile?.surname ?? "";
            VocationInput.Text = profile?.vocation ?? "";
            ResumeEditor.Text = profile?.description ?? "";

            RefreshCategoriesUI(profile);
        }

        private void RefreshCategoriesUI(AuthService.UserProfile profile = null)
        {
            CategoriesGrid.Children.Clear();
            CategoriesGrid.RowDefinitions.Clear();

            // Используем переданный профиль или текущий
            var categoriesToShow = profile?.categories ??
                                 (_currentProfile?.categories ?? new List<string>());

            if (categoriesToShow.Count == 0) return;

            int itemsPerRow = 3;
            int rows = (int)Math.Ceiling((double)categoriesToShow.Count / itemsPerRow);

            for (int r = 0; r < rows; r++)
            {
                CategoriesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < itemsPerRow; c++)
                {
                    if (index >= categoriesToShow.Count) break;

                    string categoryName = categoriesToShow[index];
                    var chip = CreateCategoryChip(categoryName);
                    CategoriesGrid.Add(chip, c, r);
                    index++;
                }
            }
        }


        // Keep only the async version that actually saves to server
        private async Task SaveProfileChanges()
        {
            try
            {
                var username = AuthService.GetUsernameAsync();
                var updates = new Dictionary<string, string>();

                // Добавляем только измененные поля
                if (NameInput.Text != _originalProfileData.Name)
                    updates["name"] = NameInput.Text;

                if (SurnameInput.Text != _originalProfileData.Surname)
                    updates["surname"] = SurnameInput.Text;

                if (VocationInput.Text != _originalProfileData.Vocation)
                    updates["vocation"] = VocationInput.Text;

                if (ResumeEditor.Text != _originalProfileData.Description)
                    updates["description"] = ResumeEditor.Text;

                // Если нет изменений - просто выходим
                if (updates.Count == 0)
                {
                    return;
                }

                bool success = await AuthService.UpdateUserProfileAsync(username, updates);

                if (success)
                {
                    // Обновляем оригинальные данные
                    _originalProfileData.Name = NameInput.Text;
                    _originalProfileData.Surname = SurnameInput.Text;
                    _originalProfileData.Vocation = VocationInput.Text;
                    _originalProfileData.Description = ResumeEditor.Text;

                    await DisplayAlert("Успех", "Изменения профиля сохранены", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось сохранить изменения", "OK");
                    RevertProfileChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении профиля: {ex}");
                await DisplayAlert("Ошибка", "Произошла ошибка при сохранении", "OK");
                RevertProfileChanges();
            }
        }

        private void RevertProfileChanges()
        {
            if (_originalProfileData == null) return;

            // Восстанавливаем оригинальные значения
            NameInput.Text = _originalProfileData.Name;
            SurnameInput.Text = _originalProfileData.Surname;
            VocationInput.Text = _originalProfileData.Vocation;
            ResumeEditor.Text = _originalProfileData.Description;

            // Обновляем UI
            UpdateProfileUI(_currentProfile);
        }

        private void AddCategoryProgrammatically(string categoryName)
        {
            if (!addedCategories.ContainsKey(categoryName))
            {
                // Добавляем категорию с пустыми тегами и навыками
                addedCategories[categoryName] = (string.Empty, string.Empty);

                // Обновляем интерфейс
                RefreshCategoriesUI();

                // Добавляем категорию в список всех доступных категорий (если нужно)
                allCategories.Add(categoryName);

                // Устанавливаем цвет для новой категории
                Color categoryColor = GetCategoryColor(categoryName);
                Console.WriteLine($"Category '{categoryName}' has color: {categoryColor}");
            }
        }


        #region Редактирование профиля

        private async void OnEditIconClicked(object sender, EventArgs e)
        {
            _isEditing = !_isEditing;
            SetProfileEditMode(_isEditing);
            ToggleEventCardTaps(!_isEditing);
            if (!_isEditing) await SaveProfileChanges();
        }

        private void SetProfileEditMode(bool isEditing)
        {
            NameInput.IsReadOnly = SurnameInput.IsReadOnly = VocationInput.IsReadOnly = ResumeEditor.IsReadOnly = !isEditing;
            AddCategoryIcon.IsVisible = SaveIcon.IsVisible = isEditing;
            EditIcon.IsVisible = !isEditing;
            AddEventIcon.Opacity = isEditing ? 0.5 : 1;
            AddEventIcon.IsEnabled = !isEditing;
            ResumeEditor.Placeholder = isEditing ? "Write about yourself..." : "Description is empty";

            // Обновляем UI категорий
            RefreshCategoriesUI(_currentProfile);

            // Включаем/выключаем кнопки удаления
            ToggleDeleteButtons(isEditing);
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

        //private void SaveProfileChanges()
        //{
        //    DisplayAlert("Success", "Profile changes saved!", "OK");
        //}

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
            if (!_isEditing) return;

            // Получаем список доступных категорий
            var availableCategories = allCategories
                .Except(_currentProfile?.categories ?? new List<string>())
                .ToArray();

            if (availableCategories.Length == 0)
            {
                await DisplayAlert("Информация", "Все категории уже добавлены", "OK");
                return;
            }

            string selectedCategory = await DisplayActionSheet("Выберите категорию", "Отмена", null, availableCategories);

            if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "Отмена")
            {
                // Для новой категории передаем пустую строку тегов
                OpenCategoryPopup(selectedCategory);
            }
        }

        private void OpenCategoryPopup(string categoryName)
        {
            CategoryPopup.IsVisible = true;
            CategoryNameLabel.Text = categoryName;

            if (categoryExamples.TryGetValue(categoryName, out var examples))
            {
                TagsEditor.Placeholder = $"Enter tags... (e.g. {examples.TagExample})";
                SkillsEditor.Placeholder = $"Describe skills... (e.g. {examples.SkillExample})";
            }

            if (addedCategories.TryGetValue(categoryName, out var data))
            {
                TagsEditor.Text = data.Tags;
                SkillsEditor.Text = data.Skills;
            }
            else
            {
                TagsEditor.Text = SkillsEditor.Text = string.Empty;
            }

            CategoryNameLabel.TextColor = GetCategoryColor(categoryName);

            if (!_isEditing)
            {
                TagsEditor.IsReadOnly = true;
                SkillsEditor.IsReadOnly = true;
                SaveCategoryButton.IsVisible = false;
            }
            else
            {
                TagsEditor.IsReadOnly = false;
                SkillsEditor.IsReadOnly = false;
                SaveCategoryButton.IsVisible = true;
            }
        }

        private void OnCloseCategoryClicked(object sender, EventArgs e)
        {
            CategoryPopup.IsVisible = false;
        }

        //private void OnSaveCategoryClicked(object sender, EventArgs e)
        //{
        //    string categoryName = CategoryNameLabel.Text;
        //    addedCategories[categoryName] = (TagsEditor.Text, SkillsEditor.Text);

        //    RefreshCategoriesUI();

        //    CategoryPopup.IsVisible = false;
        //}

        private void ToggleDeleteButtons(bool isVisible)
        {
            foreach (var frame in CategoriesGrid.Children.OfType<Frame>())
            {
                if (frame.Content is Grid grid && grid.Children.Count > 1)
                {
                    if (grid.Children[1] is ImageButton deleteButton)
                    {
                        deleteButton.IsVisible = isVisible;
                    }
                }
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
            foreach (var button in new[] { CategoryIT, CategoryCreation, CategorySport, CategoryScience, CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth })
            {
                button.BackgroundColor = Colors.DarkGray;
                button.BorderColor = Colors.Transparent;
                button.BorderWidth = 0;
            }
        }

        private void EnableCategoryButtons(bool isEnabled)
        {
            foreach (var button in new[] { CategoryIT, CategoryCreation, CategorySport, CategoryScience, CategoryBusiness, CategoryEducation, CategorySocial, CategoryHealth })
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
                _isEditingEvent = !_isEditingEvent;
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
                WidthRequest = 107,
                HeightRequest = 40
            };

            var label = new Label
            {
                Text = categoryName,
                TextColor = Colors.White,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            if (_isEditing)
            {
                var grid = new Grid();
                grid.Children.Add(label);

                var deleteButton = new ImageButton
                {
                    Source = "delete_icon.png",
                    BackgroundColor = Colors.Transparent,
                    WidthRequest = 16,
                    HeightRequest = 16,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };
                deleteButton.Clicked += async (s, e) => await DeleteCategory(categoryName);
                grid.Children.Add(deleteButton);

                frame.Content = grid;
            }
            else
            {
                frame.Content = label;
            }

            frame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OpenCategoryPopup(categoryName))
            });

            return frame;
        }

        private async Task DeleteCategory(string categoryName)
        {
            bool confirm = await DisplayAlert("Подтверждение",
                $"Удалить категорию {categoryName}?", "Да", "Нет");

            if (!confirm) return;

            try
            {
                var username = AuthService.GetUsernameAsync();
                bool success = await AuthService.RemoveProfileCategory(username, categoryName);

                if (success)
                {
                    // Обновляем профиль после удаления
                    _currentProfile = await AuthService.GetUserProfileAsync(username);
                    RefreshCategoriesUI(_currentProfile);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить категорию", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении категории: {ex}");
                await DisplayAlert("Ошибка", "Произошла ошибка при удалении", "OK");
            }
        }

        private async void OnSaveCategoryClicked(object sender, EventArgs e)
        {
            string categoryName = CategoryNameLabel.Text;
            string tags = TagsEditor.Text?.Trim() ?? "";
            string skills = SkillsEditor.Text?.Trim() ?? "";

            try
            {
                var username = AuthService.GetUsernameAsync();
                bool success = await AuthService.UpdateProfileCategory(username, categoryName, tags, skills);

                if (success)
                {
                    // Обновляем UI
                    _currentProfile = await AuthService.GetUserProfileAsync(username);
                    RefreshCategoriesUI(_currentProfile);
                    CategoryPopup.IsVisible = false;
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось сохранить изменения", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении категории: {ex}");
                await DisplayAlert("Ошибка", "Произошла ошибка при сохранении", "OK");
            }
        }

        private async void OnDeleteEventClicked(object sender, EventArgs e)
        {
            string eventHash = _originalEventHash;
            bool confirm = await DisplayAlert("Удаление мероприятия",
                $"Вы уверены, что хотите удалить мероприятие \"{EventNameInput.Text}\"?", "Да", "Нет");

            if (confirm)
            {
                bool success = await AuthService.DeleteEventAsync(eventHash);
                if (success)
                {
                    await DisplayAlert("Успех", "Мероприятие успешно удалено", "OK");
                    EventPopup.IsVisible = false;
                    await RefreshEventsFromServer(); // Обновляем данные с сервера
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить мероприятие", "OK");
                }
            }
        }

        private async void SaveEventChanges()
        {
            try
            {
                // 1. Валидация названия
                string name = EventNameInput.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    await DisplayAlert("Ошибка", "Название мероприятия не может быть пустым", "OK");
                    return;
                }

                if (name.Length > 50)
                {
                    await DisplayAlert("Ошибка", "Название слишком длинное (макс. 50 символов)", "OK");
                    return;
                }

                // 2. Валидация дат
                if (_endDate < _startDate)
                {
                    await DisplayAlert("Ошибка", "Дата окончания не может быть раньше даты начала", "OK");
                    return;
                }

                // 3. Валидация категорий
                if (selectedCategories.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Выберите хотя бы одну категорию", "OK");
                    return;
                }

                // 4. Подготовка данных
                var username = AuthService.GetUsernameAsync();
                var eventData = new AuthService.EventData
                {
                    name = name,
                    owner = username,
                    categories = selectedCategories,
                    description = EventDescriptionInput.Text?.Trim(),
                    location = EventLocationInput.Text?.Trim(),
                    date_from = _startDate.ToString("dd.MM.yyyy"),
                    date_to = _endDate.ToString("dd.MM.yyyy"),
                    @public = _isPublic ? 1 : 0,
                    online = _isOnline ? 1 : 0
                };

                bool success;
                string operation;

                // 5. Логика сохранения в зависимости от режима
                if (_isCreatingEvent)
                {
                    // Проверка на дубликаты (только для новых событий)
                    if (events.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && e.IsMyEvent))
                    {
                        await DisplayAlert("Ошибка", "Мероприятие с таким названием уже существует", "OK");
                        return;
                    }

                    operation = "создано";
                    success = await AuthService.AddEventAsync(eventData);
                }
                else
                {
                    // Для редактирования подставляем исходный hash
                    eventData.hash = _originalEventHash;
                    operation = "обновлено";
                    success = await AuthService.UpdateEventAsync(eventData); // Предполагается аналогичный метод в AuthService
                }

                // 6. Обработка результата
                if (success)
                {
                    await DisplayAlert("Успех", $"Мероприятие успешно {operation}!", "OK");
                    EventPopup.IsVisible = false;
                    await RefreshEventsFromServer();
                }
                else
                {
                    await DisplayAlert("Ошибка", $"Не удалось {operation} мероприятие", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении мероприятия: {ex}");
                await DisplayAlert("Ошибка", "Произошла непредвиденная ошибка", "OK");
            }
            finally
            {
                // Можно добавить скрытие индикатора загрузки
            }
        }

        private async Task RefreshEventsFromServer()
        {
            try
            {
                var username = AuthService.GetUsernameAsync();
                List<AuthService.EventData> got_events = await AuthService.GetAllUserEventsAsync(username);

                events.Clear();
                foreach (var eventData in got_events)
                {
                    events.Add((
                        eventData.hash,
                        eventData.name,
                        string.Join(", ", eventData.categories),
                        eventData.description,
                        eventData.location,
                        eventData.online == 1,
                        eventData.@public == 1,
                        DateTime.ParseExact(eventData.date_from, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        DateTime.ParseExact(eventData.date_to, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        eventData.owner == username
                    ));
                }

                RefreshEventsUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении мероприятий: {ex}");
                await DisplayAlert("Ошибка", "Не удалось обновить список мероприятий", "OK");
            }
        }

        private void RefreshEventsUI()
        {
            EventsContainer.Children.Clear();
            var filtered = _showMyEvents ? events.Where(ev => ev.IsMyEvent).ToList() : events.Where(ev => !ev.IsMyEvent).ToList();
            var displayList = filtered.Take(3).ToList();
            foreach (var ev in displayList)
                AddEventCard(ev.Hash, ev.Name, ev.IsMyEvent);
            ShowAllEventsButton.IsVisible = true;
        }

        private void AddEventCard(string eventHash, string eventName, bool isMyEvent)
        {
            var frame = new Frame
            {
                AutomationId = eventHash,
                BackgroundColor = Color.FromArgb("#353535"),
                CornerRadius = 10,
                Padding = 10,
                Margin = 5,
                HeightRequest = 40
            };
            var grid = new Grid();
            var label = new Label
            {
                Text = eventName,
                TextColor = Colors.White,
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center
            };
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
                events.RemoveAll(ev => ev.Hash == eventHash);
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
                    Command = new Command(() => OpenEventPopup(eventHash, false))
                });
            }
        }

        private void OpenEventPopup(string eventHash, bool isEditing)
        {
            _isCreatingEvent = string.IsNullOrEmpty(eventHash);
            var eventData = _isCreatingEvent ? default : events.FirstOrDefault(e => e.Hash == eventHash);
            if (!_isCreatingEvent && string.IsNullOrEmpty(eventData.Name))
            {
                DisplayAlert("Error", "Event not found.", "OK");
                return;
            }
            _originalEventHash = _isCreatingEvent ? "" : eventData.Hash;
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
                _isEditingEvent = false;
                SaveEventButton.Source = "edit_icon.png";
                UpdateEventPopupUI();
            }
            UpdateCategoryButtons();
            EventPopup.IsVisible = true;
        }

        private bool IsCurrentEventMine()
        {
            var ev = events.FirstOrDefault(e => e.Hash == _originalEventHash);
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
                    AutomationId = ev.Hash,
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
                            OpenEventPopup(ev.Hash, true);
                        else
                            OpenEventPopup(ev.Hash, false);
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
