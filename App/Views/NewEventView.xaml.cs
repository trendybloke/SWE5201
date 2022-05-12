using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using App.Models;
using App.Services;
using Microsoft.Extensions.DependencyInjection;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewEventView : Page
    {
        bool titleEmpty;
        bool descEmpty;
        private DataService dataService
        {
            get
            {
                return ((App)App.Current)
                            .Container
                            .GetService<DataService>();
            }
        }
        public NewEventView()
        {
            this.InitializeComponent();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (titleEmpty || descEmpty)
            {
                ErrorTextBlock.Text = "Cannot have an Empty title / description!"; 
                return;
            }

            // Split tags into array
            string[] splitTags;

            if (TagsTextBox.Text.Contains(", "))
                splitTags = TagsTextBox.Text.Split(", ");
            else
                splitTags = new string[] { TagsTextBox.Text };

            // Check if tags exist
            var allTags = (await dataService.GetAllAsync<Tag>())
                            .Select(x => new Tag
                            {
                                Id = x.Id,
                                Content = x.Content,
                                Events = x.Events
                            }
                            ).ToList();

            List<Tag> newTags = new List<Tag>();

            foreach (var newTag in splitTags)
            {
                if (string.IsNullOrEmpty(newTag))
                    continue;

                bool tagExists = false;

                foreach (var tag in allTags)
                {
                    if (tag.Content.ToLower() == newTag.ToLower())
                    {
                        tagExists = true;
                        break;
                    }
                }

                if (tagExists)
                {
                    newTags.Add(
                                    allTags
                                    .Where(t => t.Content.ToLower() == newTag.ToLower())
                                    .FirstOrDefault()
                               );
                }
                else
                {
                    Tag newTagObj = new Tag
                    {
                        Content = newTag
                    };

                    newTags.Add(await dataService.InsertAsync<Tag>(newTagObj));

                    if(!dataService.LastResponse.IsSuccessStatusCode)
                    {
                        ErrorTextBlock.Text = "Unable to add new event.";
                        return;
                    }
                }
            }

            Event newEvent = new Event()
            {
                Title = TitleTextBox.Text,
                Description = DescriptionTextBox.Text,
                Tags = newTags
            };

            var result = await dataService.InsertAsync<Event>(newEvent);
            if (!dataService.LastResponse.IsSuccessStatusCode)
            {
                ErrorTextBlock.Text = "Unable to add new event.";
                return;
            }
            else
            {
                ContentDialog successDialog = new ContentDialog()
                {
                    Title = "Success",
                    Content = "New Event added",
                    CloseButtonText = "Ok"
                };

                await successDialog.ShowAsync();
            }
        }

        private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            descEmpty = string.IsNullOrEmpty(DescriptionTextBox.Text);
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            titleEmpty = string.IsNullOrEmpty(TitleTextBox.Text);
        }
    }
}
