using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ComponentModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using App.Models;
using App.Enums;
using System.Text;
using App.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowseEventsView : Page, INotifyPropertyChanged
    {
        
        private DataService dataService
        {
            get
            {
                return ((App)App.Current)
                            .Container
                            .GetService<DataService>();
            }
        }
        
        private List<Event> Events;

        private Event _lastEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        public BrowseEventsView()
        {
            this.RefreshEventList("", "");
            this.InitializeComponent();
        }

        private Event GetSelectedEvent()
        {
            //return HostedEvents[EventList.SelectedIndex];
            return Events.ElementAt(EventList.SelectedIndex);
        }

        private Visibility manageUsersVisibility;
        public Visibility ManageUsersVisibility
        {
            get
            {
                return this.manageUsersVisibility;
            }
            set
            {
                this.manageUsersVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManageUsersVisibility)));
            }
        }

        private async void RefreshEventList(string titleFilter, string tagsFilter)
        {
            
            var allEvents =
                (await dataService.GetAllAsync<Event>())
                .Select(x => new Event
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Tags = x.Tags
                }).ToList();

            if (!string.IsNullOrEmpty(titleFilter))
                allEvents = allEvents
                                .Where(x => x.Title.Contains(titleFilter))
                                .ToList();

            if (!string.IsNullOrEmpty(tagsFilter))
            {
                string[] tags;

                if (tagsFilter.Contains(", "))
                    tags = tagsFilter.Split(", ");
                else
                    tags = new string[] { tagsFilter };

                foreach(var tag in tags)
                {
                    allEvents = allEvents
                                .Where(x => x.Tags
                                    .Any(t => t.Content.Contains(tag))
                                ).ToList();
                }
            }

            Events = allEvents;

        }

        private void RefreshContent(Event viewedEvent)
        {
            // Populate Content page
            TitleTextBlock.Text = viewedEvent.Title;
            TagsTextBlock.Text = $"Tags: {viewedEvent.TagsString}";
            DescriptionTextBlock.Text = viewedEvent.Description;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.RefreshEventList("", "");
                //RefreshContent(GetSelectedEvent());
                EventList.ItemsSource = Events;
                EventList.SelectedIndex = 0;
                _lastEvent = GetSelectedEvent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Events != null && EventList.SelectedIndex >= 0)
            {
                RefreshContent(GetSelectedEvent());
                _lastEvent = GetSelectedEvent();
            }
        }

        private void TitleFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.RefreshEventList(TitleFilterTextBox.Text, TagFilterTextBox.Text);
            EventList.ItemsSource = Events;
        }

        private void TagFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.RefreshEventList(TitleFilterTextBox.Text, TagFilterTextBox.Text);
            EventList.ItemsSource = Events;
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var container = new StackPanel();

            TextBox titleBox = new TextBox()
            {
                Text = _lastEvent.Title,
                Header = "Title"
            };

            TextBox tagsBox = new TextBox()
            {
                Text = _lastEvent.TagsString == "None" ? "" : _lastEvent.TagsString,
                PlaceholderText = "Tag, Tag, Tag...",
                Header = "Tags"
            };

            TextBox descBox = new TextBox()
            {
                Text = _lastEvent.Description,
                Header = "Description"
            };

            container.Children.Add(titleBox);
            container.Children.Add(tagsBox);
            container.Children.Add(descBox);

            ContentDialog editEventDialog = new ContentDialog()
            {
                Title = "Edit Event",
                Content = container,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult editedEvent = await editEventDialog.ShowAsync();

            if(editedEvent == ContentDialogResult.Primary)
            {
                List<Tag> editedTags = new List<Tag>();

                // Is tagsBox empty?
                if (!string.IsNullOrEmpty(tagsBox.Text))
                {
                    // Split tags into array
                    string[] splitTags;
                
                    if(tagsBox.Text.Contains(", "))
                        splitTags = tagsBox.Text.Split(", ");
                    else
                        splitTags = new string[]{ tagsBox.Text };

                    // Check if tags exist
                    var allTags = (await dataService.GetAllAsync<Tag>())
                                    .Select(x => new Tag
                                    {
                                        Id = x.Id,
                                        Content = x.Content,
                                        Events = x.Events
                                    }
                                    ).ToList();

                    foreach(var newTag in splitTags)
                    {
                        if(string.IsNullOrEmpty(newTag))
                            continue;

                        bool tagExists = false;

                        foreach(var tag in allTags)
                        {
                            if (tag.Content.ToLower() == newTag.ToLower())
                            {
                                tagExists = true;
                                break;
                            }
                        }

                        Tag editedTag;
    
                        // If they don't, save new one to database
                        if (!tagExists)
                        {
                            // Format content to 'Content'
                            StringBuilder formNewTag = new StringBuilder();
                            formNewTag.Append(newTag.ToUpper());
                            formNewTag.Remove(1, newTag.Length - 1);
                            formNewTag.Append(newTag.ToLower());
                            formNewTag.Remove(1, 1);
    
                            editedTag = new Tag()
                            {
                                Content = formNewTag.ToString()
                            };
    
                            var result = await dataService
                                                .InsertAsync<Tag>(editedTag);
                        }
                        // If they do exist, get data from allTags and update that tag
                        else
                        {
                            editedTag = allTags
                                            .Where(x => x.Content.ToLower() == newTag.ToLower())
                                            .FirstOrDefault();
    
                            editedTag.Events.Add(new Event()
                            {
                                Id = _lastEvent.Id,
                                Title = titleBox.Text,
                                Description = descBox.Text
                            });
    
                            var result = await dataService
                                                .UpdateAsync<Tag, int>(editedTag, editedTag.Id);
    
                        }
                        if (!dataService.LastResponse.IsSuccessStatusCode)
                        {
                            var ErrorDialog = new ContentDialog()
                            {
                                Title = "Could not update Event.",
                                PrimaryButtonText = "Ok"
                            };

                            await ErrorDialog.ShowAsync();
                            return;
                        }

                        // Add them to editedTags
                        editedTags.Add(editedTag);
                    }

                }
                else
                {

                }
                // Build new event
                Event newEvent = new Event()
                {
                    Id = _lastEvent.Id,
                    Title = titleBox.Text,
                    Description = descBox.Text,
                    Tags = editedTags
                };

                // Save Event to API
                var save = await dataService
                                    .UpdateAsync<Event, int>
                                    (newEvent, newEvent.Id);

                if (dataService.LastResponse.IsSuccessStatusCode)
                {
                    RefreshEventList(TitleFilterTextBox.Text, TagFilterTextBox.Text);
                    RefreshContent(GetSelectedEvent());
                    EventList.ItemsSource = Events;
                    _lastEvent = newEvent;

                    var SuccessDialog = new ContentDialog()
                    {
                        Title = "Event updated!",
                        PrimaryButtonText = "Ok"
                    };

                    await SuccessDialog.ShowAsync();
                }
                else
                {
                    var ErrorDialog = new ContentDialog()
                    {
                        Title = "Could not update Event.",
                        PrimaryButtonText = "Ok"
                    };

                    await ErrorDialog.ShowAsync();
                }
            }
        }

        private async void HostButton_Click(object sender, RoutedEventArgs e)
        {
            var container = new StackPanel();

            // Get Rooms
            var roomObjs = (await dataService.GetAllAsync<Room>()).ToList();
            string[] rooms = new string[roomObjs.Count];

            for(int i = 0; i < rooms.Length; i++)
                rooms[i] = roomObjs[i].Name;

            var roomComboBox = new ComboBox()
            {
                ItemsSource = rooms,
                Header = "Room"
            };

            var timePicker = new TimePicker()
            {
                Header = "Time"
            };

            var datePicker = new DatePicker()
            {
                Header = "Date"
            };

            var durationBox = new TextBox()
            {
                Header = "Duration (Days, Hours:Minutes)",
                PlaceholderText = "x, xx:xx"
            };

            var feeNumberBox = new NumberBox()
            {
                Header = "Entrance fee (£x.xx)"
            };

            container.Children.Add(roomComboBox);
            container.Children.Add(timePicker);
            container.Children.Add(datePicker);
            container.Children.Add(durationBox);
            container.Children.Add(feeNumberBox);

            ContentDialog hostEventDialog = new ContentDialog()
            {
                Title = $"Schedule {_lastEvent.Title}",
                Content = container,
                PrimaryButtonText = "Schedule",
                CloseButtonText = "Close"
            };

            bool validInput = false;
            while (!validInput)
            {
                var result = await hostEventDialog.ShowAsync();

                if (result != ContentDialogResult.Primary)
                    break;

                // Check input format, display error if incorrect
                ContentDialog incorrectInput = new ContentDialog()
                {
                    Title = "Incorrect input",
                    CloseButtonText = "Ok"
                };

                // Room has to be selected
                if(roomComboBox.SelectedIndex == -1)
                {
                    incorrectInput.Content = "You need to select a room";
                    await incorrectInput.ShowAsync();
                    continue;
                }

                // Date cannot be in the past
                DateTime startTime;

                startTime = new DateTime(datePicker.Date.DateTime.Year,
                        datePicker.Date.DateTime.Month,
                        datePicker.Date.DateTime.Day,
                        timePicker.Time.Hours,
                        timePicker.Time.Minutes,
                        timePicker.Time.Seconds);

                if(startTime.CompareTo(DateTime.Now) == -1)
                {
                    incorrectInput.Content = "Cannot pick a Date in the past.";
                    await incorrectInput.ShowAsync();
                    continue;
                }

                // Duration cannot be empty or equal zero
                string[] splitByComma = durationBox.Text.Split(",");
                string[] splitByCommaThenColon = splitByComma[1].Split(":");

                bool validDay = int.TryParse(splitByComma[0], out int durDay);
                bool validHours = int.TryParse(splitByCommaThenColon[0], out int durHour);
                bool validMin = int.TryParse(splitByCommaThenColon[1], out int durMin);

                if(!validDay || !validHours || !validMin)
                {
                    incorrectInput.Content = $"Incorrect format for Duration{Environment.NewLine}" +
                        $"Ensure what you entered follows the format 'Days, Hours:Minutes'";
                    await incorrectInput.ShowAsync();
                    continue;
                }

                // Fee cannot be negative (assume empty means zero)
                bool validFee = float.TryParse(feeNumberBox.Text, out float fee);
                if(!validFee || fee < 0)
                {
                    incorrectInput.Content = "Entered fee is not a number. Make sure '£' is excluded.";
                    await incorrectInput.ShowAsync();
                    continue;
                }

                Debug.WriteLine(_lastEvent.Id);

                // Build HostedEvent
                HostedEvent newHostedEvent = new HostedEvent()
                {
                    Event = _lastEvent,
                    Room = roomObjs[roomComboBox.SelectedIndex],
                    StartTime = startTime,
                    DurationMinutes = durMin,
                    DurationHours = durHour,
                    DurationDays = durDay,
                    EntranceFee = fee
                };

                // Post
                var postResult = await dataService.InsertAsync<HostedEvent>(newHostedEvent);
                if (!dataService.LastResponse.IsSuccessStatusCode)
                {
                    ContentDialog failed = new ContentDialog()
                    {
                        Title = "Failed to schedule event",
                        Content = dataService.LastResponse.StatusCode,
                        CloseButtonText = "Ok"
                    };

                    await failed.ShowAsync();
                    continue;
                }
                ContentDialog scheduled = new ContentDialog()
                {
                    Title = "Sucess",
                    Content = "Scheduled the event!",
                    CloseButtonText = "Ok"
                };

                await scheduled.ShowAsync();

                validInput = true;
                
            }
        }
    }
}
