using CommunityToolkit.Maui.Views;
using SLON.Models;

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
            if (ev.IsPublic)
            {
                PublicButton.BackgroundColor = Color.FromArgb("#915AC5");
                PrivateButton.BackgroundColor = Color.FromArgb("#515151");
            }
            else
            {
                PublicButton.BackgroundColor = Color.FromArgb("#515151");
                PrivateButton.BackgroundColor = Color.FromArgb("#915AC5");
            }

            if (ev.IsOnline)
            {
                OnlineButton.BackgroundColor = Color.FromArgb("#915AC5");
                OfflineButton.BackgroundColor = Color.FromArgb("#515151");
            }
            else
            {
                OnlineButton.BackgroundColor = Color.FromArgb("#515151");
                OfflineButton.BackgroundColor = Color.FromArgb("#915AC5");
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
