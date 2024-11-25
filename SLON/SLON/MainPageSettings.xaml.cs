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

        // ������ CollectionView ����������
        var collectionView = new CollectionView
        {
            ItemsSource = categories,
            SelectionMode = SelectionMode.None
        };

        // ������������� DataTemplate ��� ���������
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

            // ����������� ����� ������
            button.SetBinding(Button.TextProperty, ".");

            // ��������� ���� ������ � ����������� �� ���������
            button.BindingContextChanged += (sender, e) =>
            {
                if (sender is Button btn)
                {
                    if (selectedCategories.Contains((string)btn.BindingContext))
                    {
                        btn.BackgroundColor = Color.FromRgb(61, 61, 61); // ��������� ���������
                    }
                    else
                    {
                        btn.BackgroundColor = Color.FromRgb(217, 217, 217); // �� ������� ���������
                    }
                }
            };

            // ��������� ���������� �������
            button.Clicked += OnCategorySelected;

            return button;
        });


        if (verticalStackLayout != null)
        {
            verticalStackLayout.Children.Insert(1, collectionView); // ������ 1 � ����� �������� CollectionView ����� ���������
        }
        else
        {
            Debug.WriteLine("verticalStackLayout �� ������.");
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        // ��������� ��������� ��������� � Settings
        Settings.selectedCategories = new HashSet<string>(selectedCategories); // ����� ������� ����� ���������
        Close();
    }

    private void OnCategorySelected(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            // �����/������ ������ ���������
            if (!selectedCategories.Contains(button.Text))
            {
                button.BackgroundColor = Color.FromRgb(61, 61, 61); // ��������� ���������
                selectedCategories.Add(button.Text);
            }
            else
            {
                button.BackgroundColor = Color.FromRgb(217, 217, 217); // �� ������� ���������
                selectedCategories.Remove(button.Text);
            }

            // �������� ������� ��������� ����� ������� ������
            Debug.WriteLine("Current selected categories: " + string.Join(", ", selectedCategories));
        }
    }
}
