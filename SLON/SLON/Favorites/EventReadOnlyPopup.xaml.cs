using CommunityToolkit.Maui.Views;
using SLON.Models;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace SLON
{
    public partial class EventReadOnlyPopup : Popup
    {
        public EventReadOnlyPopup(Event ev)
        {
            InitializeComponent();
            BindEvent(ev);
        }

        private void BindEvent(Event ev)
        {
            EventNameEntry.Text = ev.Name;
            CategoriesLabel.Text = string.Join(", ", ev.Categories);
            EventDescriptionEditor.Text = ev.Info;
            StartDateLabel.Text = ev.StartDate.ToString("dd.MM.yyyy");
            EndDateLabel.Text = ev.EndDate.ToString("dd.MM.yyyy");
            EventLocationEditor.Text = ev.Place;
            UpdateLocationDisplay();
            if (ev.IsPublic)
            {
                PublicButton.BackgroundColor = Color.FromArgb("#915AC5");
                PrivateButton.BackgroundColor = Colors.DarkGray;
            }
            else
            {
                PublicButton.BackgroundColor = Colors.DarkGray;
                PrivateButton.BackgroundColor = Color.FromArgb("#915AC5");
            }

            if (ev.IsOnline)
            {
                OnlineButton.BackgroundColor = Color.FromArgb("#915AC5");
                OfflineButton.BackgroundColor = Colors.DarkGray;
            }
            else
            {
                OnlineButton.BackgroundColor = Colors.DarkGray;
                OfflineButton.BackgroundColor = Color.FromArgb("#915AC5");
            }
        }

        private void UpdateLocationDisplay()
        {
            var formattedString = new FormattedString();
            var text = EventLocationEditor.Text;

            if (string.IsNullOrEmpty(text))
            {
                EventLocationLabel.FormattedText = formattedString;
                return;
            }

            var urlRegex = new Regex(@"(https?://[^\s]+)");
            var matches = urlRegex.Matches(text);

            int lastPos = 0;
            foreach (Match match in matches)
            {
                if (match.Index > lastPos)
                {
                    formattedString.Spans.Add(new Span
                    {
                        Text = text.Substring(lastPos, match.Index - lastPos),
                        TextColor = Colors.White
                    });
                }

                var urlSpan = new Span
                {
                    Text = match.Value,
                    TextColor = Color.FromArgb("#00BFFF"), // Голубой цвет
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
                            await CloseAsync();
                            await App.Current.MainPage.DisplayAlert("Error", "Could not open link", "OK");
                        }
                    })
                });

                formattedString.Spans.Add(urlSpan);
                lastPos = match.Index + match.Length;
            }

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


        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
