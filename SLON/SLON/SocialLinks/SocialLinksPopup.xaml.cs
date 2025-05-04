using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;
using SLON.Services;
using SLON.SocialLinks;

namespace SLON.SocialLinks
{
    public partial class SocialLinksPopup : ContentView
    {
        private List<SocialLinkItem> socialLinks = new();

        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                _isEditable = value;
                UpdateUIForMode();
            }
        }
        private bool _isEditable = false;

        public SocialLinksPopup()
        {
            InitializeComponent();
        }

        private void UpdateUIForMode()
        {
            NewLinkEntry.IsVisible = _isEditable;
            AddLinkButton.IsVisible = _isEditable;


            // скрыть кнопки удаления в CollectionView
            if (LinksCollectionView.ItemsSource is List<SocialLinkItem> items)
            {
                // пересоздаём коллекцию, чтобы DataTemplate отреагировал
                LinksCollectionView.ItemsSource = null;
                LinksCollectionView.ItemsSource = items;
            }
        }



        /// <summary>
        /// Обработчик для кнопки Close в XAML.
        /// </summary>
        void OnCloseLinksPopup(object sender, EventArgs e)
        {
            Hide();
        }

        /// <summary>
        /// Устанавливает список ссылок и обновляет CollectionView.
        /// </summary>
        public void SetLinks(IEnumerable<string> links)
        {
            socialLinks = links.Select(url => new SocialLinkItem(url, GetIconForUrl(url), IsEditable)).ToList();

            LinksCollectionView.ItemsSource = null;
            LinksCollectionView.ItemsSource = socialLinks;
        }


        private string GetIconForUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "url.png";

            try
            {
                var host = new Uri(url).Host.ToLower();

                if (host.Contains("discord")) return "discord.png";
                if (host.Contains("facebook")) return "facebook.png";
                if (host.Contains("github")) return "github.png";
                if (host.Contains("instagram")) return "instagram.png";
                if (host.Contains("t.me")) return "telegram.png";
                if (host.Contains("vk")) return "vk.png";
                if (host.Contains("whatsapp")) return "whatsapp.png";
                if (host.Contains("clashroyale")) return "clashroyale.png";

                return "url.png";
            }
            catch
            {
                return "url.png";
            }
        }


        /// <summary>Показывает попап.</summary>
        public async Task Show(string username)
        {
            PART_Popup.IsVisible = true;

            List<string> links = await AuthService.GetProfileUrlsAsync(username);
            Console.WriteLine("Show: ");
            links.ForEach(Console.WriteLine);
            SetLinks(links);
        }

        /// <summary>Скрывает попап.</summary>
        public void Hide() => PART_Popup.IsVisible = false;

        public IEnumerable<SocialLinkItem> GetLinks() => socialLinks;

        private async void OnAddSocialLinkClicked(object sender, EventArgs e)
        {
            var url = NewLinkEntry.Text?.Trim();
            SocialLinkItem li = new SocialLinkItem(url, GetIconForUrl(url), IsEditable);

            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                socialLinks.Add(li);
                LinksCollectionView.ItemsSource = null;
                LinksCollectionView.ItemsSource = socialLinks;
                NewLinkEntry.Text = "";

                bool isSuccess = await AuthService.SetProfileUrlsAsync(socialLinks.Select(u => u.Url));

                if (!isSuccess)
                {
                    Application.Current.MainPage.DisplayAlert("Error", "Server error. Please try again.", "OK");
                    socialLinks.Remove(li);
                }
            }
            else
                Application.Current.MainPage.DisplayAlert("Error", "Enter the correct URL", "OK");
        }
        private async void OnDeleteSocialLinkClicked(object sender, EventArgs e)
        {
            if (!(sender is ImageButton btn && btn.CommandParameter is string urlToDelete))
                return;

            // Находим и временно удаляем элемент из локальной коллекции
            var item = socialLinks.FirstOrDefault(x => x.Url == urlToDelete);
            if (item == null)
                return;

            socialLinks.Remove(item);
            LinksCollectionView.ItemsSource = null;
            LinksCollectionView.ItemsSource = socialLinks;

            // Отправляем на сервер обновлённый список без удалённого URL‑а
            bool isSuccess = await AuthService.SetProfileUrlsAsync(socialLinks.Select(x => x.Url));

            if (!isSuccess)
            {
                // Если неуспешно — откатываем удаление
                socialLinks.Add(item);

                LinksCollectionView.ItemsSource = null;
                LinksCollectionView.ItemsSource = socialLinks;

                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "The link could not be deleted. Please try again.",
                    "OK"
                );
            }
        }



        private async void OnSocialLinkTapped(object sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.GestureRecognizers[0] is TapGestureRecognizer tap &&
                tap.CommandParameter is string url &&
                Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                await Launcher.OpenAsync(uri);
            }
        }

    }

    public class SocialLinkItem(string url, string iconSource = "url.png", bool isVisible = false)
    {
        public string Url { get; set; } = url;
        public string IconSource { get; set; } = iconSource;
        public bool IsVisible { get; set; } = isVisible;
    }


}