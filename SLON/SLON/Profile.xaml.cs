using System.Globalization;
using System.Xml;

namespace SLON;

public partial class Profile : ContentPage
{
    public Profile()
    {
        InitializeComponent();
        PopulateTagSelectionList();
    }

    private bool _isEditing = false;

    /// <summary>
    /// ����������� ����� �������������� ��� ���������� ��������� ����� � ��������� ��������� ����������.
    /// </summary>
    private void OnEditIconClicked(object sender, EventArgs e)
    {
        _isEditing = !_isEditing;

        // ��������/��������� ����������� �������������� �����.
        NameInput.IsReadOnly = !_isEditing;
        VocationInput.IsReadOnly = !_isEditing;
        ResumeEditor.IsReadOnly = !_isEditing;

        // ����������� ��������� ��������� ����������.
        AddTagIcon.IsVisible = _isEditing;
        EditIcon.IsVisible = !_isEditing;
        SaveIcon.IsVisible = _isEditing;

        EnableTagEditMode(_isEditing);

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
        if (!_isEditing)
        {
            return;
        }
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
    private void EnableTagEditMode(bool isEditing)
    {
        foreach (var child in TagsContainer.Children)
        {
            if (child is Frame frame && frame.Content is Grid grid &&
                grid.Children.FirstOrDefault() is ImageButton deleteIcon)
            {
                deleteIcon.IsVisible = isEditing;
            }
        }
    }

    /// <summary>
    /// ��������� ������� �� ������ ���������� ������ ����
    /// </summary>
    private void OnAddTagIconClicked(object sender, EventArgs e)
    {
        TagSelectionList.IsVisible = !TagSelectionList.IsVisible;
    }

    /// <summary>
    /// ���������� ���� ��� ������� �� ������ � ��������� ����.
    /// </summary>
    private void OnTagButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button tagButton && tagButton.Text != null)
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

        deleteButton.Clicked += (s, e) => TagsContainer.Children.Remove(tagFrame);

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
        var tags = new List<string>
        {
            "Tester", "System administrator", "Mobile developer", "Game developer",
            "Frontend developer", "Backend developer", "Fullstack developer",
            "Data Analyst", "Data Scientist"
        };

        TagSelectionList.ItemsSource = tags;
    }

    private void SaveProfileChanges()
    {
        DisplayAlert("Success", "Profile changes saved!", "OK");
    }
}
