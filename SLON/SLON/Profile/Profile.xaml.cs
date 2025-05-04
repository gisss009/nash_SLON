using Microsoft.Maui.Controls;
using SLON.Services;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using SLON.Models;
using System.Text.RegularExpressions;
using SLON.Models;
using ImageCropper.Maui;
using SLON.SocialLinks;


namespace SLON
{
    public partial class Profile : ContentPage
    {
        private bool _isEditing = false;          // Режим редактирования профиля
        private bool _isEditingEvent = false;     // Режим редактирования события
        private bool _isCreatingEvent = false;    // Режим создания нового события
        private string _fromPage = string.Empty;
        private string _requestedUsername = string.Empty;
        private AuthService.UserProfile _currentProfile;
        private UserProfileEditModel _originalProfileData;
        private bool _isForeignProfile = false;

        // Переключатель In/My: true – мои события, false – события, где я участвую
        private bool _showMyEvents = true;

        // Списки категорий и событий
        private readonly List<string> allCategories = new() { "IT", "Creation", "Sport", "Science", "Business", "Education", "Social", "Health" };
        private readonly Dictionary<string, (string Tags, string Skills)> addedCategories = new();
        // События: (Hash, Name, Categories, Description, Location, IsOnline, IsPublic, StartDate, EndDate, IsMyEvent)
        private readonly List<(string Hash, string Name, string Categories, string Description, string Location,
                               bool IsOnline, bool IsPublic, DateTime StartDate, DateTime EndDate, bool IsMyEvent)> events = new();
        private readonly List<string> selectedCategories = new();

        private List<string> socialLinks = new List<string>();

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

        private ImageSource _originalAvatarSource;



        public Profile()
        {

            InitializeComponent();
            UpdateButtonColorsProfile(); // Первоначальная установка цветов


            StartDatePicker.MinimumDate = DateTime.Today;
            EndDatePicker.MinimumDate = DateTime.Today;
            ResumeEditor.Placeholder = "Description is empty";
            EventLocationInput.TextChanged += EventLocationInput_TextChanged;


            RefreshEventsUI();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine(_fromPage);
            bool isForeign = _fromPage == "FavoritesPage";
            Shell.SetTabBarIsVisible(this, !isForeign);
        }


        public void UpdateButtonColorsProfile()
        {
            // Для кнопок in/my
            MyEventsButton.SetDynamicResource(Button.BackgroundColorProperty,
                _showMyEvents ? "ActiveButtonColorProfile" : "ButtonColorProfile");
            InEventsButton.SetDynamicResource(Button.BackgroundColorProperty,
                !_showMyEvents ? "ActiveButtonColorProfile" : "ButtonColorProfile");

            MyEventsButton.SetDynamicResource(Button.BorderColorProperty, "ButtonBorderColorProfile");
            InEventsButton.SetDynamicResource(Button.BorderColorProperty, "ButtonBorderColorProfile");
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (_fromPage == "FavoritesPage")
            {
                UsernameLabel.GestureRecognizers.Clear();
                EditIcon.IsVisible = false;
                AddEventIcon.IsVisible = false;

                Shell.SetTabBarIsVisible(this, false);
            }
            _isForeignProfile = _fromPage == "FavoritesPage";
            UrlIcon.IsVisible = !_isForeignProfile;

            _ = LoadProfileAsync();
            
        }

        private async Task LoadProfileAsync()
        {
            // 1) Решаем, кого грузим:
            string usernameToLoad =
                _fromPage == "FavoritesPage" && !string.IsNullOrWhiteSpace(_requestedUsername)
                ? _requestedUsername
                : AuthService.GetUsernameAsync();

            // 2) Получаем профиль
            var profile = await AuthService.GetUserProfileAsync(usernameToLoad);
            _currentProfile = profile;

            TagColorConverter.TagToCategory.Clear();
            if (profile.tags != null)
            {
                foreach (var kv in profile.tags)
                {
                    string category = kv.Key;
                    // profile.tags[kv.Key] — это строка "tag1, tag2, tag3"
                    foreach (var raw in kv.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var tag = raw.Trim();
                        if (!string.IsNullOrEmpty(tag))
                            TagColorConverter.TagToCategory[tag] = category;
                    }
                }
            }

            foreach (var item in profile.categories)
            {
                Console.WriteLine(item);
            }
            if (profile == null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await DisplayAlert("Ошибка", "Не удалось загрузить профиль", "OK"));
                return;
            }

            Console.WriteLine(profile.username);
            var eventsData = await AuthService.GetAllUserEventsAsync(usernameToLoad);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                NavigationPage.SetBackButtonTitle(this, "@" + usernameToLoad);

                ExitAccIcon.IsVisible = usernameToLoad == AuthService.GetUsernameAsync();
                UsernameLabel.Text = profile.username;
                NameInput.Text = profile.name;
                SurnameInput.Text = profile.surname;
                VocationInput.Text = profile.vocation;
                ResumeEditor.Text = profile.description;

                _originalProfileData = new UserProfileEditModel
                {
                    Name = profile.name,
                    Surname = profile.surname,
                    Vocation = profile.vocation,
                    Description = profile.description,
                    Categories = profile.categories ?? new(),
                    Urls = profile.urls
                };

                UpdateProfileUI(profile);
                await LoadAndRefreshEvents(usernameToLoad);
                await LoadAndUpdateAvatar(usernameToLoad);

                _originalAvatarSource = AvatarButton.Source;
            });
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("fromPage", out var fromPageObj))
                _fromPage = fromPageObj?.ToString() ?? string.Empty;

            if (query.TryGetValue("username", out var userObj))
                _requestedUsername = userObj?.ToString() ?? string.Empty;
        }

        public async Task LoadAndUpdateAvatar(string username)
        {
            var avatarImage = await AuthService.GetUserAvatarAsync(username);

            AvatarButton.Source = avatarImage ?? ImageSource.FromFile("default_profile_icon.png");
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
            SLON.Models.Settings.Save();

            Settings.Init("");
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
            public List<string> Urls { get; set; }
        }

        private void UpdateProfileUI(AuthService.UserProfile profile)
        {
            UsernameLabel.Text = profile?.username ?? "";
            NameInput.Text = profile?.name ?? "";
            SurnameInput.Text = profile?.surname ?? "";
            VocationInput.Text = profile?.vocation ?? "";
            ResumeEditor.Text = profile?.description ?? "";

            UpdateButtonColorsProfile();

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

        private async Task SaveProfileChanges()
        {
            try
            {
                var username = AuthService.GetUsernameAsync();
                var updates = new Dictionary<string, string>();

                if (NameInput.Text != _originalProfileData.Name)
                    updates["name"] = NameInput.Text;

                if (SurnameInput.Text != _originalProfileData.Surname)
                    updates["surname"] = SurnameInput.Text;

                if (VocationInput.Text != _originalProfileData.Vocation)
                    updates["vocation"] = VocationInput.Text;

                if (ResumeEditor.Text != _originalProfileData.Description)
                    updates["description"] = ResumeEditor.Text;

                if (updates.Count == 0)
                {
                    return;
                }

                bool success = await AuthService.UpdateUserProfileAsync(username, updates);

                if (success)
                {
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

            NameInput.Text = _originalProfileData.Name;
            SurnameInput.Text = _originalProfileData.Surname;
            VocationInput.Text = _originalProfileData.Vocation;
            ResumeEditor.Text = _originalProfileData.Description;

            UpdateProfileUI(_currentProfile);
        }

        private async void OnUrlIconClicked(object sender, EventArgs e)
        {
            LinksPopupCtrl.IsEditable = true;

            await LinksPopupCtrl.Show(AuthService.GetUsernameAsync());
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

            if (isEditing)
                AvatarButton.Source = "edit_profile_avatar.png";
            else
                AvatarButton.Source = _originalAvatarSource;

            RefreshCategoriesUI(_currentProfile);
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
                new ImageCropper.Maui.ImageCropper()
                {
                    PageTitle = "Test Title",
                    AspectRatioX = 1,
                    AspectRatioY = 1,
                    CropShape = ImageCropper.Maui.ImageCropper.CropShapeType.Oval,
                    SelectSourceTitle = "Select source",
                    TakePhotoTitle = "Take Photo",
                    PhotoLibraryTitle = "Photo Library",
                    CancelButtonTitle = "Cancel",
                    Success = async (imageFile) =>
                    {
                        // Preview locally
                        Dispatcher.Dispatch(() =>
                        {
                            AvatarButton.Source = ImageSource.FromFile(imageFile);
                        });

                        // Read cropped image into byte array
                        byte[] data;
                        using (var stream = File.OpenRead(imageFile))
                        {
                            using var ms = new MemoryStream();
                            await stream.CopyToAsync(ms);
                            data = ms.ToArray();
                        }

                        // Upload to server
                        var ok = await AuthService.UploadUserAvatarAsync(data, Path.GetFileName(imageFile));
                        if (!ok)
                        {
                            await DisplayAlert("Error", "Failed to upload avatar", "OK");
                            return;
                        }

                        // Reload from server
                        //string username = AuthService.GetUsernameAsync();
                        //var url = new Uri($"http://139.28.223.134:5000/photos/image/{Uri.EscapeDataString(username)}");
                        Dispatcher.Dispatch(() =>
                        {
                            AvatarButton.Source = ImageSource.FromFile(imageFile);
                        });
                    }
                }.Show(this);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
            }
        }



        #endregion

        #region Управление категориями

        private async void OnAddCategoryIconClicked(object sender, EventArgs e)
        {
            if (!_isEditing) return;

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

            CategoryNameLabel.TextColor = GetCategoryColor(categoryName);

            if (categoryExamples.TryGetValue(categoryName, out var examples))
            {
                TagsEditor.Placeholder = $"Enter tags... (e.g. {examples.TagExample})";
                SkillsEditor.Placeholder = $"Describe skills... (e.g. {examples.SkillExample})";
            }

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

            UpdatePublicPrivateButtonsUI();
            UpdateOnlineOfflineButtonsUI();

            EventLocationInput.Placeholder = "Venue...";
            ResetCategoryButtons();
            SaveEventButton.Source = (String)Application.Current.Resources["SaveButton"];

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
                UpdateLocationDisplay();
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
                    await RefreshEventsFromServer();
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
            bool wasCreating = _isCreatingEvent;

            if ((DateTime.Now - _lastSaveTime).TotalSeconds < 1)
                return;
            _lastSaveTime = DateTime.Now;

            try
            {
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

                bool isDuplicate = wasCreating
                    ? events.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    : events.Any(e => e.Hash != _originalEventHash
                                      && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (isDuplicate)
                {
                    await DisplayAlert("Ошибка", "Мероприятие с таким именем уже существует", "OK");
                    return;
                }

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
                    hash = wasCreating ? null : _originalEventHash
                };

                AuthService.EventData resultEvent = wasCreating
                    ? await AuthService.AddEventAsync(payload)
                    : await AuthService.UpdateEventAsync(payload);

                if (resultEvent != null)
                {
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

                    string successMsg = wasCreating
                        ? "Мероприятие успешно создано!"
                        : "Мероприятие успешно обновлено!";
                    await DisplayAlert("Успех", successMsg, "OK");

                    // Сброс флага создания и закрытие попапа
                    _isCreatingEvent = false;
                    EventPopup.IsVisible = false;
                    RefreshEventsUI();
                    UpdateLocationDisplay();
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
                if (ex is TaskCanceledException)
                {
                    await DisplayAlert("Ошибка", "Время ожидания истекло. Возможно, сервер не отвечает.", "OK");
                }
                else if (ex is HttpRequestException httpEx)
                {
                    await DisplayAlert("Ошибка сети", $"Проблема с сетью: {httpEx.Message}", "OK");
                }
                else
                {
                    await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
                }
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

            var sorted = filtered
                .OrderBy(ev => ev.StartDate)
                .ToList();

            foreach (var ev in sorted.Take(3))
                AddEventCard(ev.Hash, ev.Name, ev.IsMyEvent);
            ShowAllEventsButton.IsVisible = true;

            UpdateButtonColorsProfile();
        }


        private void AddEventCard(string eventHash, string eventName, bool isMyEvent)
        {
            var frame = new Frame
            {
                AutomationId = eventHash,
                BackgroundColor = (Color)Application.Current.Resources["EventsColorProfile"],
                CornerRadius = 10,
                Padding = 10,
                Margin = 5,
                HeightRequest = 50
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
            // разрешаем редактировать только свои ивенты в своём профиле
            bool canEditEvent = !_isForeignProfile && eventData.IsMyEvent;
            if (!canEditEvent)
            // Вот это новенькое место:
            UpdatePublicPrivateButtonsUI();
            UpdateOnlineOfflineButtonsUI();

            if (!eventData.IsMyEvent)
            {
                SaveEventButton.IsVisible = false;
                DeleteEventButton.IsVisible = false;

                // превращаем все поля в readonly
                EventNameInput.IsReadOnly = true;
                EventDescriptionInput.IsReadOnly = true;
                EventLocationInput.IsReadOnly = true;
                EnableCategoryButtons(false);
                StartDatePicker.IsEnabled = false;
                EndDatePicker.IsEnabled = false;
            }
            else
            {
                // свой ивент в своём профиле — показываем возможность редактировать
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
            EventPopupHeaderLabel.Text = _isCreatingEvent ? "Create event card" : (_isEditingEvent ? "Edit event card" : "Event card");
            SaveEventButton.Source = _isEditingEvent ? "save_icon.png" : "edit_icon.png";
            EventNameInput.IsReadOnly = !_isEditingEvent;
            EventDescriptionInput.IsReadOnly = !_isEditingEvent;
            EventLocationInput.IsReadOnly = !_isEditingEvent;
            bool canEditDates = _isEditingEvent || _isCreatingEvent;
            StartDatePicker.IsEnabled = EndDatePicker.IsEnabled = canEditDates;
            EnableCategoryButtons(canEditDates);
            bool canEdit = _isEditingEvent || _isCreatingEvent;
            EventLocationInput.IsReadOnly = !canEdit;
            EventLocationInput.IsEnabled = canEdit;
            UpdateLocationDisplay();
            UpdateCategoryButtons();
        }

        private void OnLocationButtonClicked(object sender, EventArgs e)
        {
            if (!(_isEditingEvent || _isCreatingEvent)) return;

            _isOnline = sender == OnlineButton;
            UpdateOnlineOfflineButtonsUI();
        }


        private void OnPublicPrivateButtonClicked(object sender, EventArgs e)
        {
            if (!(_isEditingEvent || _isCreatingEvent)) return;

            _isPublic = sender == PublicButton;
            UpdatePublicPrivateButtonsUI();
        }


        private void UpdatePublicPrivateButtonsUI()
        {
            if (_isPublic)
            {
                PublicButton.SetDynamicResource(Button.BackgroundColorProperty, "ActiveButtonColorProfile");
                PrivateButton.SetDynamicResource(Button.BackgroundColorProperty, "ButtonColorProfile");
            }
            else
            {
                PrivateButton.SetDynamicResource(Button.BackgroundColorProperty, "ActiveButtonColorProfile");
                PublicButton.SetDynamicResource(Button.BackgroundColorProperty, "ButtonColorProfile");
            }
        }
        private void UpdateLocationDisplay()
        {
            bool isEditable = _isEditingEvent || _isCreatingEvent;
            EventLocationInput.IsVisible = isEditable;
            EventLocationLabel.IsVisible = !isEditable;
            if (!isEditable)
            {
                var formattedString = new FormattedString();
                var text = EventLocationInput.Text;

                if (string.IsNullOrEmpty(text))
                {
                    EventLocationLabel.FormattedText = formattedString;
                    return;
                }

                // Регулярное выражение для поиска URL
                var urlRegex = new Regex(@"(https?://[^\s]+)");
                var matches = urlRegex.Matches(text);

                int lastPos = 0;
                foreach (Match match in matches)
                {
                    // Текст до ссылки
                    if (match.Index > lastPos)
                    {
                        formattedString.Spans.Add(new Span
                        {
                            Text = text.Substring(lastPos, match.Index - lastPos),
                            TextColor = Colors.White
                        });
                    }

                    // Сама ссылка
                    var urlSpan = new Span
                    {
                        Text = match.Value,
                        TextColor = Color.FromArgb("#00BFFF"),
                        TextDecorations = TextDecorations.Underline
                    };

                    urlSpan.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            try
                            {
                                await Launcher.OpenAsync(match.Value);
                            }
                            catch
                            {
                                await DisplayAlert("Error", "Could not open link", "OK");
                            }
                        })
                    });

                    formattedString.Spans.Add(urlSpan);
                    lastPos = match.Index + match.Length;
                }

                // Остаток текста после последней ссылки
                if (lastPos < text.Length)
                {
                    formattedString.Spans.Add(new Span
                    {
                        Text = text.Substring(lastPos),
                        TextColor = Colors.White
                    });
                }

                EventLocationLabel.FormattedText = formattedString;
            }
        }

        private void UpdateOnlineOfflineButtonsUI()
        {
            if (_isOnline)
            {
                OnlineButton.SetDynamicResource(Button.BackgroundColorProperty, "ActiveButtonColorProfile");
                OfflineButton.SetDynamicResource(Button.BackgroundColorProperty, "ButtonColorProfile");
                EventLocationInput.Placeholder = "Link to the meeting...";
            }
            else
            {
                OfflineButton.SetDynamicResource(Button.BackgroundColorProperty, "ActiveButtonColorProfile");
                OnlineButton.SetDynamicResource(Button.BackgroundColorProperty, "ButtonColorProfile");
                EventLocationInput.Placeholder = "Venue...";
            }
        }



        private void OnCancelEventClicked(object sender, EventArgs e)
        {
            EventPopup.IsVisible = false;
            UpdateLocationDisplay();
        }
        private void EventLocationInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EventLocationLabel.IsVisible)
            {
                UpdateLocationDisplay();
            }
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
                    button.BorderColor = Colors.Transparent;
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
        InEventsButton.SetDynamicResource(Button.BackgroundColorProperty, "ActiveButtonColorProfile");
        MyEventsButton.SetDynamicResource(Button.BackgroundColorProperty, "ButtonColorProfile");
    }
    else if (sender == MyEventsButton)
    {
        _showMyEvents = true;
        MyEventsButton.SetDynamicResource(Button.BackgroundColorProperty, "ActiveButtonColorProfile");
        InEventsButton.SetDynamicResource(Button.BackgroundColorProperty, "ButtonColorProfile");
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
                    BackgroundColor = (Color)Application.Current.Resources["BlockColorProfile"],
                    CornerRadius = 10,
                    Padding = 10,
                    Margin = 3,
                    HeightRequest = 50
                };
                var grid = new Grid();
                var label = new Label
                {
                    Text = ev.Name,
                    TextColor = (Color)Application.Current.Resources["TextColorProfile"],
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
