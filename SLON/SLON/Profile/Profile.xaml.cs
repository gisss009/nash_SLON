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


        private async void OnUsernameTapped(object sender, EventArgs e)
        {
            string username = UsernameLabel.Text;

            bool confirm = await DisplayAlert("Подтверждение",
                 $"Вы уверены, что хотите выйти из аккаунта {username}?", "Да", "Нет");

            if (!confirm)
                return;

            // Очищаем учетные данные
            await AuthService.ClearCredentialsAsync();
            AuthService.SetAuthenticated(false);

            // Очищаем локальные коллекции избранного
            Models.Favourites.ResetFavorites();

            // Переходим на экран авторизации
            Application.Current.MainPage = new NavigationPage(new AuthPage());
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
            UsernameLabel.Text = profile?.username ?? "";
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

            if (isEditing)
            {
                NameInput.TextColor = Colors.LightGray;
                SurnameInput.TextColor = Colors.LightGray;
                VocationInput.TextColor = Colors.LightGray;
                ResumeEditor.TextColor = Colors.LightGray;
            }
            else
            {
                NameInput.TextColor = Colors.White;
                SurnameInput.TextColor = Colors.White;
                VocationInput.TextColor = Colors.White;
                ResumeEditor.TextColor = Colors.White;
            }

            AvatarButton.Source = isEditing ? "edit_profile_avatar.png" : "default_profile_icon.png";

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
                OpenCategoryPopup(selectedCategory);
            }
        }

        private void OpenCategoryPopup(string categoryName)
        {
            CategoryPopup.IsVisible = true;
            CategoryNameLabel.Text = categoryName;

            // Устанавливаем цвет категории
            CategoryNameLabel.TextColor = GetCategoryColor(categoryName);

            // Получаем примеры для подсказок
            if (categoryExamples.TryGetValue(categoryName, out var examples))
            {
                TagsEditor.Placeholder = $"Enter tags... (e.g. {examples.TagExample})";
                SkillsEditor.Placeholder = $"Describe skills... (e.g. {examples.SkillExample})";
            }

            // Получаем текущие теги и навыки для этой категории из профиля
            if (_currentProfile?.tags != null && _currentProfile.tags.TryGetValue(categoryName, out var currentTags))
            {
                TagsEditor.Text = currentTags;
            }
            else
            {
                TagsEditor.Text = string.Empty;
            }

            if (_currentProfile?.skills != null && _currentProfile.skills.TryGetValue(categoryName, out var currentSkills))
            {
                SkillsEditor.Text = currentSkills;
            }
            else
            {
                SkillsEditor.Text = string.Empty;
            }

            // Настройка режима редактирования
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

            _isCreatingEvent = true;
            _isEditingEvent = false;
            _originalEventHash = string.Empty;  

            EventPopupHeaderLabel.Text = "Create event card";
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

        private async void OnEditSaveEventClicked(object sender, EventArgs e)
        {
            if (_isCreatingEvent || _isEditingEvent)
            {
                await SaveEventChanges();
                return;
            }

            if (IsCurrentEventMine())
            {
                _isEditingEvent = !_isEditingEvent;
                SaveEventButton.Source = _isEditingEvent ? "save_icon.png" : "edit_icon.png";
                EventPopupHeaderLabel.Text = _isEditingEvent ? "Edit event card" : "Event card";
                UpdateEventPopupUI();
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

            // Проверка
            if (string.IsNullOrWhiteSpace(tags) || string.IsNullOrWhiteSpace(skills))
            {
                await DisplayAlert("Error", "Both Tags and Skills fields must be filled.", "OK");
                return;
            }

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

        private DateTime _lastSaveTime = DateTime.MinValue;

        private async Task SaveEventChanges()
        {
            // 0) Запомним, создавали ли мы ивент
            bool wasCreating = _isCreatingEvent;

            // Анти-дубль-таймер
            if ((DateTime.Now - _lastSaveTime).TotalSeconds < 1)
                return;
            _lastSaveTime = DateTime.Now;

            try
            {
                // --- Сбор и валидация полей ---
                string name = EventNameInput.Text?.Trim() ?? "";
                string description = EventDescriptionInput.Text?.Trim() ?? "";
                string location = EventLocationInput.Text?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(name)
                    || string.IsNullOrWhiteSpace(description)
                    || string.IsNullOrWhiteSpace(location)
                    || selectedCategories.Count == 0)
                {
                    await DisplayAlert("Ошибка", "Все поля должны быть заполнены", "OK");
                    return;
                }

                if (name.Length > 50)
                {
                    await DisplayAlert("Ошибка", "Название слишком длинное (макс. 50 символов)", "OK");
                    return;
                }

                if (_endDate < _startDate)
                {
                    await DisplayAlert("Ошибка", "Дата окончания не может быть раньше даты начала", "OK");
                    return;
                }

                // --- Проверка дубликата в локальном списке ---
                bool isDuplicate = wasCreating
                    ? events.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    : events.Any(e => e.Hash != _originalEventHash
                                      && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (isDuplicate)
                {
                    await DisplayAlert("Ошибка", "Мероприятие с таким именем уже существует", "OK");
                    return;
                }

                // --- Подготовка данных для API ---
                var username = AuthService.GetUsernameAsync();
                var payload = new AuthService.EventData
                {
                    name = name,
                    owner = username,
                    categories = new List<string>(selectedCategories),
                    description = description,
                    location = location,
                    date_from = _startDate.ToString("dd.MM.yyyy"),
                    date_to = _endDate.ToString("dd.MM.yyyy"),
                    @public = _isPublic ? 1 : 0,
                    online = _isOnline ? 1 : 0,
                    // Передаём hash только при обновлении
                    hash = wasCreating ? null : _originalEventHash
                };

                // --- Вызов API создания или обновления ---
                AuthService.EventData resultEvent = wasCreating
                    ? await AuthService.AddEventAsync(payload)
                    : await AuthService.UpdateEventAsync(payload);

                // --- Обработка ответа ---
                if (resultEvent != null)
                {
                    // Обновляем локальную коллекцию events
                    var idx = events.FindIndex(e => e.Hash == resultEvent.hash);
                    var tuple = (
                        resultEvent.hash,
                        resultEvent.name,
                        string.Join(", ", resultEvent.categories),
                        resultEvent.description,
                        resultEvent.location,
                        resultEvent.online == 1,
                        resultEvent.@public == 1,
                        DateTime.ParseExact(resultEvent.date_from, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        DateTime.ParseExact(resultEvent.date_to, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        true
                    );

                    if (idx >= 0)
                        events[idx] = tuple;
                    else
                        events.Add(tuple);

                    // Показать правильное сообщение
                    string successMsg = wasCreating
                        ? "Мероприятие успешно создано!"
                        : "Мероприятие успешно обновлено!";
                    await DisplayAlert("Успех", successMsg, "OK");

                    // Сброс флага создания и закрытие попапа
                    _isCreatingEvent = false;
                    EventPopup.IsVisible = false;
                    RefreshEventsUI();
                }
                else
                {
                    // Обработка неудачи
                    string failMsg = wasCreating
                        ? "Не удалось создать мероприятие"
                        : "Не удалось обновить мероприятие";
                    await DisplayAlert("Ошибка", failMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении мероприятия: {ex}");
                await DisplayAlert("Ошибка", "Произошла неожиданная ошибка", "OK");
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

            var filtered = _showMyEvents
                ? events.Where(ev => ev.IsMyEvent)
                : events.Where(ev => !ev.IsMyEvent);

            // Сортируем по дате начала
            var sorted = filtered
                .OrderBy(ev => ev.StartDate)
                .ToList();

            // Показываем только три первых
            foreach (var ev in sorted.Take(3))
                AddEventCard(ev.Hash, ev.Name, ev.IsMyEvent);

            // Кнопка «Показать все» видна, только если больше трёх
            // ShowAllEventsButton.IsVisible = sorted.Count > 3;
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
                // HeightRequest = 40
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
            // Если eventHash пустой, значит это режим создания.
            _isCreatingEvent = string.IsNullOrEmpty(eventHash);

            if (_isCreatingEvent)
            {
                // Если создаем новое событие, устанавливаем заголовок:
                EventPopupHeaderLabel.Text = "Create Event card";
            }
            else if (_isEditingEvent)
            {
                EventPopupHeaderLabel.Text = "Edit Event card";
            }
            else
            {
                EventPopupHeaderLabel.Text = "Event card";
            }

            var eventData = _isCreatingEvent ? default : events.FirstOrDefault(e => e.Hash == eventHash);
            if (!_isCreatingEvent && string.IsNullOrEmpty(eventData.Name))
            {
                DisplayAlert("Error", "Event not found.", "OK");
                return;
            }
            _originalEventHash = _isCreatingEvent ? string.Empty : eventData.Hash;

            // Заполняем поля данными или очищаем для создания нового события.
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

            if (sorted.Count == 0)
            {
                var emptyLayout = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 20
                };

                var emptyImage = new Image
                {
                    Source = "slon.png",
                    WidthRequest = 350,
                    HeightRequest = 350,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                var messageLabel = new Label
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 21 
                };

                if (_showMyEvents)
                {
                    messageLabel.Text = "Create an event in profile!";
                }
                else
                {
                    messageLabel.Text = "Swipe events to participate!";
                }

                emptyLayout.Children.Add(emptyImage);
                emptyLayout.Children.Add(messageLabel);

                AllEventsContainer.Children.Add(emptyLayout);
                return;
            }

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
