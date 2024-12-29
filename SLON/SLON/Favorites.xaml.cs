using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using SLON.Models;

namespace SLON;

public partial class Favorites : ContentPage
{
    public ObservableCollection<User> favorites = Favourites.favorites;
    private Image starIcon;

    public Favorites()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        Debug.WriteLine("NAVIGATE");
        Debug.WriteLine("COUNT: " + favorites.Count);

        LoadFavorites();
    }

    private void LoadFavorites()
    {
        Debug.WriteLine($"count posle: {favorites.Count}");
        favoritesPanel.Children.Clear();
        //Image starIcon;
        if (favorites.Count == 0)
        {
            Frame frame = new Frame();
            Image image = new Image
            {
                Source = ImageSource.FromFile("Resources/Images/slon.png"),
                WidthRequest = 300,
                HeightRequest = 300,
                VerticalOptions = LayoutOptions.Center
            };
            image.Margin = new Thickness(0, DeviceInfo.Platform == DevicePlatform.iOS ? 20 : 100, 0, 0);

           
            Label firstLine = new Label
            {

                Text = "Your likes will appear here",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 28,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromRgb(225, 225, 225)

            };
            firstLine.Margin = new Thickness(0, DeviceInfo.Platform == DevicePlatform.iOS ? 20 : 0, 0, 0);

            Label secondLine = new Label
            {

                Text = "Swipe cards to add events",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromRgba("#B8B8B8")

            };
            secondLine.Margin = new Thickness(0, DeviceInfo.Platform == DevicePlatform.iOS ? 20 : 0, 0, 0);

            frame.Content = new StackLayout
            {
                Children = {
                    image,
                    firstLine,
                    secondLine
                }
            };

            favoritesPanel.Children.Add(image);
            favoritesPanel.Children.Add(firstLine);
            favoritesPanel.Children.Add(secondLine);
        }
        else
        {
            foreach (var user in favorites)
            {
                Debug.WriteLine("ADDED IN BUTTON: " + user.Name);

                Frame userFrame = new Frame
                {
                    Padding = new Thickness(5),
                    BackgroundColor = Color.FromArgb("#222222"),
                    Margin = new Thickness(5),
                    CornerRadius = 10
                };

                Grid userGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(60) },
                        new ColumnDefinition { Width = GridLength.Star }
                    }
                };

                Image userIcon = new Image
                {
                    Source = ImageSource.FromFile("Resources/Images/default_profile_icon1.png"),
                    WidthRequest = 50,
                    HeightRequest = 50,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start
                };
                Grid.SetColumn(userIcon, 0);

                Label userLabel = new Label
                {
                    Text = $"{user.Name}\n{user.Vocation}",
                    TextColor = Colors.White,
                    FontSize = 16,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start
                };
                Grid.SetColumn(userLabel, 1);

                starIcon = new Image
                {
                    Source = ImageSource.FromFile("Resources/Images/empty_star.png"),
                    WidthRequest = 50,
                    HeightRequest = 50,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.End
                };
                Grid.SetColumn(starIcon, 1);
                

                userGrid.Children.Add(userIcon);
                userGrid.Children.Add(userLabel);
                userGrid.Children.Add(starIcon);

                TapGestureRecognizer tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnUserTapped(user);
                userGrid.GestureRecognizers.Add(tapGesture);

                userFrame.Content = userGrid;

                favoritesPanel.Children.Add(userFrame);
            }
        }
    }

    private async void OnUserTapped(User selectedUser)
    {
        // Получаем starIcon через родительский элемент, чтобы изменить его источник
        var userFrame = favoritesPanel.Children
            .OfType<Frame>()
            .FirstOrDefault(f => f.Content is Grid userGrid && userGrid.Children.OfType<Label>().Any(l => l.Text.Contains(selectedUser.Name)));

        if (userFrame != null)
        {
            var userGrid = (Grid)userFrame.Content;
            var starIcon = userGrid.Children.OfType<Image>().LastOrDefault(); // Получаем последний элемент, который является starIcon

            if (starIcon != null)
            {
                // Меняем источник изображения
                if (starIcon.Source.ToString().Contains("empty_star.png"))
                {
                    starIcon.Source = ImageSource.FromFile("Resources/Images/fill_star.png");
                }
                else
                {
                    starIcon.Source = ImageSource.FromFile("Resources/Images/empty_star.png");
                }
            }
        }

        //await DisplayAlert("User Info", $"You clicked on {selectedUser.Name}", "OK");
    }

    //private async void OnUserTapped(User selectedUser)
    //{

    //    await DisplayAlert("User Info", $"You clicked on {selectedUser.Name}", "OK");
    //}
};

