using System.Globalization;
using System.Xml;

namespace SLON;

public partial class Profile : ContentPage
{
    private bool _isEditing = false;

    // ������ ��������� �����
    private readonly List<string> allTags = new()
    {
        "Tester", "System administrator", "Mobile developer", "Game developer",
        "Frontend developer", "Backend developer", "Fullstack developer",
        "Data Analyst", "Data Scientist"
    };

    // ������ ��� �������� ��������� �����
    private readonly List<string> selectedTags = new();

    public Profile()
    {
        InitializeComponent();
        PopulateTagSelectionList();
    }

    /// <summary>
    /// ����������� ����� �������������� ��� ���������� ��������� ����� � ��������� ��������� ����������.
    /// </summary>
    private void OnEditIconClicked(object sender, EventArgs e)
    {
        _isEditing = !_isEditing;

        // ����������� ��������� �������������� ���������
        NameInput.IsReadOnly = ResumeEditor.IsReadOnly = VocationInput.IsReadOnly = !_isEditing;
        AddTagIcon.IsVisible = SaveIcon.IsVisible = _isEditing;
        EditIcon.IsVisible = !_isEditing;

        // ��������/��������� ����� �������������� �����
        EnableTagEditMode();

        if (!_isEditing)
        {
            SaveProfileChanges();
        }
    }

    /// <summary>
    /// ��������� ������ ������������
    /// </summary>
    private async void OnAvatarButtonClicked(object sender, EventArgs e)
    {
        if (!_isEditing) return;

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "�������� �����������:",
                FileTypes = FilePickerFileType.Images
            });

            if (result != null)
            {
                AvatarButton.Source = ImageSource.FromFile(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� ��������� �����������: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// ���������/���������� ������ �������������� �����
    /// </summary>
    private void EnableTagEditMode()
    {
        foreach (var frame in TagsContainer.Children.OfType<Frame>())
        {
            if (frame.Content is Grid grid && grid.Children.FirstOrDefault() is ImageButton deleteIcon)
            {
                deleteIcon.IsVisible = _isEditing;
            }
        }
    }

    /// <summary>
    /// ��������� ������� �� ������ ���������� ������ ����
    /// </summary>
    private void OnAddTagIconClicked(object sender, EventArgs e)
    {
        TagSelectionList.IsVisible = !TagSelectionList.IsVisible;

        if (TagSelectionList.IsVisible)
        {
            PopulateTagSelectionList();
        }
    }

    /// <summary>
    /// ���������� ���� ��� ������� �� ������ � ��������� ����.
    /// </summary>
    private void OnTagButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button tagButton && !string.IsNullOrWhiteSpace(tagButton.Text))
        {
            AddTagToContainer(tagButton.Text);
            TagSelectionList.IsVisible = false;
        }
    }

    /// <summary>
    /// ���������� ���������� ���� �� ������.
    /// </summary>
    private void OnTagSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedTag)
        {
            AddTagToContainer(selectedTag);
            TagSelectionList.IsVisible = false;
        }
    }

    /// <summary>
    /// ���������� ������ ���� � ��������� �����.
    /// </summary>
    private void AddTagToContainer(string tagName)
    {
        if (selectedTags.Contains(tagName)) return;

        selectedTags.Add(tagName);

        var tagFrame = new Frame
        {
            BackgroundColor = Colors.Gray,
            CornerRadius = 5,
            Padding = new Thickness(5, 2),
            Margin = new Thickness(5),
            HasShadow = false
        };

        var tagGrid = new Grid
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
            IsVisible = _isEditing,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };

        deleteButton.Clicked += (s, e) =>
        {
            TagsContainer.Children.Remove(tagFrame);
            selectedTags.Remove(tagName);
            PopulateTagSelectionList();
        };

        var tagLabel = new Label
        {
            Text = tagName,
            TextColor = Colors.White,
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };

        tagGrid.Children.Add(deleteButton);
        Grid.SetColumn(deleteButton, 0);
        tagGrid.Children.Add(tagLabel);
        Grid.SetColumn(tagLabel, 1);

        tagFrame.Content = tagGrid;

        TagsContainer.Children.Add(tagFrame);
    }

    /// <summary>
    /// ���������� ������ ��������� ��� ������ �����.
    /// </summary>
    private void PopulateTagSelectionList()
    {
        var availableTags = allTags.Except(selectedTags).ToList();
        TagSelectionList.ItemsSource = availableTags;
    }

    private void SaveProfileChanges()
    {
        DisplayAlert("Success", "Profile changes saved!", "OK");
    }
}


