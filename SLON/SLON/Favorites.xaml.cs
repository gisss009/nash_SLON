using System.Collections.Generic;
using Microsoft.Maui.Controls;
using SLON.Models;
using Syncfusion.Maui.Core.Internals;
using static Microsoft.Maui.Controls.Button.ButtonContentLayout;
namespace SLON;

public partial class Favorites : ContentPage
{
    public Favorites()
    {
        InitializeComponent();
        LoadFavorites();
    }
    private void LoadFavorites()
    {
       
        HashSet<User> favorites = Favourites.favorites;
        //favorites.Add(new User(3, "Carla Perez", "IT, Creation", "Frontend Developer", "React expert with a focus on responsive design", "JavaScript, React, CSS"));
        //favorites.Add(new User(4, "David Lee", "IT, Science", "Data Scientist", "Experienced in AI and machine learning", "Python, TensorFlow, PyTorch"));
        //favorites.Add(new User(1, "Alice Johnson", "IT, Creation", "UI Designer", "Specialist in mobile app design", "Adobe XD, Figma, Photoshop"));
        //favorites.Add(new User(2, "Bob Smith", "IT", "Backend Developer", "Focused on high-performance APIs", "C#, .NET, SQL"));

        favoritesPanel.Children.Clear();

        if (favorites.Count == 0)
        {
            Image image = new Image
            {
                Source = ImageSource.FromFile("Resources/Images/slon.png"),
                WidthRequest = 300,
                HeightRequest = 300,
                VerticalOptions = LayoutOptions.Center
            };
            image.Margin = new Thickness(0, DeviceInfo.Platform == DevicePlatform.iOS ? 20 : 100, 0, 0);
            

            // Если HashSet пуст, выводим:
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
            Content = new StackLayout
            {
                Children = {
                    image,
                    firstLine,
                    secondLine
            
            
            }
            };
        }

        else
        {
            // Если есть элементы, создаем кнопки для каждого пользователя
            foreach (var user in favorites)
            {
                

                Button userButton = new Button
                {
                    ImageSource = ImageSource.FromFile("Resources/Images/account.png"),

                    ContentLayout = new Button.ButtonContentLayout(ImagePosition.Left, 100),
                    Text = user.Name + "\n" + user.Vocation, 
                    BackgroundColor = Color.FromArgb("#222222"),
                    Padding = 5,
                    Margin = new Thickness(5),
                    CornerRadius = 10
                };

                
                // обработчик события нажатия на кнопку
                userButton.Clicked += UserButton_Click;


                favoritesPanel.Children.Add(userButton);
                this.Content = favoritesPanel;
            }
        }
    }

    private async void UserButton_Click(object sender, EventArgs e)
    {
        
    }
}


