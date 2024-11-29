using System.Collections.Generic;
using Microsoft.Maui.Controls;
using SLON.Models;
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
        User us1 = new User(1, "Alice Johnson", "IT, Creation", "UI Designer", "Specialist in mobile app design", "Adobe XD, Figma, Photoshop");
        User us2 = new User(2, "Bob Smith", "IT", "Backend Developer", "Focused on high-performance APIs", "C#, .NET, SQL");

    // Предположим, что у вас есть статическое поле favorites в классе Favorites
        HashSet<User> favorites = Favourites.favorites;
        favorites.Add(new User(3, "Carla Perez", "IT, Creation", "Frontend Developer", "React expert with a focus on responsive design", "JavaScript, React, CSS"));
        favorites.Add(new User(4, "David Lee", "IT, Science", "Data Scientist", "Experienced in AI and machine learning", "Python, TensorFlow, PyTorch"));
        favorites.Add(new User(5, "Emma Brown", "Business, Creation", "Marketing Manager", "Specialist in social media campaigns", "SEO, Content Marketing, Google Ads"));
        favorites.Add(new User(6, "Frank Wilson", "IT, Business", "DevOps Engineer", "Focus on CI/CD pipelines", "Docker, Kubernetes, Jenkins"));
        favorites.Add(new User(7, "Grace Adams", "Business, Education", "Project Manager", "Certified Scrum Master", "Agile, Scrum, Kanban"));
        favorites.Add(new User(8, "Henry Carter", "IT, Business", "Blockchain Developer", "Expert in smart contracts", "Solidity, Ethereum, Web3"));
        favorites.Add(new User(9, "Ivy Martinez", "Creation, Education", "Content Writer", "Crafts engaging stories", "Copywriting, Blogging, SEO Writing"));
        favorites.Add(new User(10, "Jack Robinson", "IT, Science", "Cybersecurity Specialist", "Focus on network security", "Penetration Testing, Firewalls, Ethical Hacking"));
        // Очищаем предыдущие элементы в StackLayout
        favoritesPanel.Children.Clear();

        if (favorites.Count == 0)
        {
            Frame frame = new Frame {
                BackgroundColor = Color.FromArgb("#3C3C3C"),
                Padding = 1,
                Margin = new Thickness(20, 300, 20, 300),
                CornerRadius = 20
            };
            // Если HashSet пуст, выводим текст
            Label emptyMessage = new Label
            {
                
                Text = "It's still empty here.\nIt's time to start the search!",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromRgb(225, 225, 225)

            };
            emptyMessage.Margin = new Thickness(0, DeviceInfo.Platform == DevicePlatform.iOS ? 20 : 0, 0, 0);
            frame.Content = emptyMessage;
            favoritesPanel.Children.Add(emptyMessage);
            Content = frame;
        }
        else
        {
            // Если есть элементы, создаем кнопки для каждого пользователя
            foreach (var user in favorites)
            {
                /*Image image = new Image
                {
                    Source = ImageSource.FromFile("Resources/Images/account.png"),
                    WidthRequest = 60,
                    HeightRequest = 60,
                    VerticalOptions = LayoutOptions.Center
                };*/
                
                Button userButton = new Button
                {
                    Text = user.Name +"\n" +user.Vocation, // Предположим, что у User есть свойство Name
                    ImageSource = ImageSource.FromFile("Resources/Images/account.png"),
                 
                    ContentLayout = new Button.ButtonContentLayout(ImagePosition.Left, 100),
                    BackgroundColor = Color.FromArgb("#222222"),
                    Padding = 5,
                    Margin = new Thickness(5),
                    CornerRadius = 10
                };

                /*userButton.Content = "Favorite Button";

                // Установка BindingContext для кнопки
                //userButton.BindingContext = user;*/

                // Добавляем обработчик события нажатия на кнопку
                userButton.Clicked += UserButton_Click;


                // Добавляем кнопку в VerticalStackLayout
                favoritesPanel.Children.Add(userButton);
                this.Content = favoritesPanel;
            }
        }
    }
 
    private async void UserButton_Click(object sender, EventArgs e)
    {
        // Обработка нажатия на кнопку, например, открытие информации о пользователе
        Button clickedButton = sender as Button;
        User selectedUser = clickedButton.BindingContext as User;

        if (selectedUser != null)
            {
                await DisplayAlert("User Info", $"You clicked on {selectedUser.Name}", "OK");
            }
    }
}
