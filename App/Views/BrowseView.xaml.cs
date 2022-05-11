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
using System.Text;
using App.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowseView : Page, INotifyPropertyChanged
    {
        /*
        private DataService dataService
        {
            get
            {
                return ((App)App.Current)
                            .Container
                            .GetService<DataService>();
            }
        }
        */
        private List<HostedEvent> HostedEvents;

        private bool _eventsAreUpcoming;

        public event PropertyChangedEventHandler PropertyChanged;

        public BrowseView()
        {
            this.InitializeComponent();
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            //HostedEvents = dataService.GetAllAsync<HostedEvent>().Result;
        }

        private HostedEvent GetSelectedEvent()
        {
            //return HostedEvents[EventList.SelectedIndex];
            return HostedEvents.ElementAt(EventList.SelectedIndex);
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

        private async void RefreshEventList()
        {
            /*
            HostedEvents =
                (await dataService.GetAllAsync<HostedEvent>())
                .Select(x => new HostedEvent
                {
                    Id = x.Id,
                    Event = x.Event,
                    Room = x.Room,
                    StartTime = x.StartTime,
                    DurationMinutes = x.DurationMinutes,
                    DurationHours = x.DurationHours,
                    DurationDays = x.DurationDays,
                    EntranceFee = x.EntranceFee
                }).ToList();
            */

            List<HostedEvent> allEvents = new List<HostedEvent>()
            {
                new HostedEvent()
                {
                    Id = 1,
                    Event = new Event()
                    {
                        Id = 1,
                        Title = "Stand up Comedy night",
                        Description = "Come along for a night of laughs",
                        Tags = new List<Tag>()
                        {
                            new Tag()
                            {
                                Id = 1,
                                Content = "Comedy"
                            },
                            new Tag()
                            {
                                Id = 2,
                                Content = "Live"
                            }
                        }
                    },
                    Room = new Room()
                    {
                        Id = 1,
                        Name = "M1-11",
                        Capacity = 150
                    },
                    StartTime = new DateTime(2021, 9, 12, 16, 0, 0),
                    DurationMinutes = 30,
                    DurationHours = 2,
                    DurationDays = 0,
                    EntranceFee = 0.5f
                },
                new HostedEvent()
                {
                    Id = 2,
                    Event = new Event()
                    {
                        Id = 1,
                        Title = "Stand up Comedy night",
                        Description = "Come along for a night of laughs",
                        Tags = new List<Tag>()
                        {
                            new Tag()
                            {
                                Id = 1,
                                Content = "Comedy"
                            },
                            new Tag()
                            {
                                Id = 2,
                                Content = "Live"
                            }
                        }
                    },
                    Room = new Room()
                    {
                        Id = 1,
                        Name = "M1-11",
                        Capacity = 150
                    },
                    StartTime = new DateTime(2022, 9, 12, 16, 0, 0),
                    DurationMinutes = 30,
                    DurationHours = 2,
                    DurationDays = 0,
                    EntranceFee = 0.5f
                }
            };

            if (_eventsAreUpcoming)
            {
                var upcomingEvents = allEvents.Where(x => x.StartTime.CompareTo(DateTime.Now) == 1);
                HostedEvents = upcomingEvents.ToList<HostedEvent>();
                //PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(HostedEvents)));
                return;
            }

            HostedEvents = allEvents;
        }

        private void RefreshContent(HostedEvent viewedEvent)
        {
            // Populate Content page
            TitleTextBlock.Text = viewedEvent.Event.Title;
            RoomTextBlock.Text = $"Room: {viewedEvent.Room.Name}";
            TagsTextBlock.Text = $"Tags: {viewedEvent.Event.TagsString}";

            DateTime start = viewedEvent.StartTime;
            DateTime end = viewedEvent.EndTime;

            StringBuilder sb = new StringBuilder();
            sb.Append($"{start.ToString("HH:mm")} - ");
            sb.Append($"{end.ToString("HH:mm")} ");
            sb.Append($" on {start.ToString("dddd, dd/MM/yyyy")}");

            if (viewedEvent.DurationDays > 0)
                sb.Append($" until {end.ToString("dddd, dd/MM/yyyy")}");

            DateTextBlock.Text = sb.ToString();

            DurationTextBlock.Text = $"Duration: {viewedEvent.DurationString}";

            if (viewedEvent.EntranceFee > 0)
                FeeTextBlock.Text =
                    // Represents entrance fee as a price in £.
                    "Entrance Fee: " + viewedEvent.EntranceFee.ToString(
                        "C", new CultureInfo("en-GB", false).NumberFormat
                    );
            else
                FeeTextBlock.Text = "No Entrance Fee";

            DescriptionTextBlock.Text = viewedEvent.Event.Description;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshEventList();
            //RefreshContent(GetSelectedEvent());
            EventList.ItemsSource = HostedEvents;
        }

        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(HostedEvents != null)
                RefreshContent(GetSelectedEvent());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string wantedEvents = (e.Parameter as NavigationViewItem).Content.ToString().Split(" ")[1];

            bool clickedUpcoming =
                wantedEvents == "Upcoming";

            _eventsAreUpcoming = clickedUpcoming;
        }

        private async void BookButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected event
            HostedEvent selectedEvent = GetSelectedEvent();

            // Confimation dialog
            ContentDialog confirmBook = new ContentDialog()
            {
                Title = "Confirm booking?",
                Content = $"Room {selectedEvent.Room} " +
                $"at {selectedEvent.StartTime.ToString("HH:mm")} " +
                $"on {selectedEvent.StartTime.ToString("dddd, dd/MM/yyyy")}",
                PrimaryButtonText = "Confirm",
                SecondaryButtonText = "Cancel"
            };

            // Already booked dialog
            ContentDialog alreadyBooked = new ContentDialog()
            {
                Title = "Unable to book",
                Content = "You're already booked to see this event!",
                CloseButtonText = "Oops"
            };

            // Show confirm dialog
            var userAction = await alreadyBooked.ShowAsync();

            // If Primary Button selected
            if (userAction == ContentDialogResult.Primary)
            {
                // Book event
                return;
            }
            // If Secondary Button selected
            else
            {
                // Return
                return;
            }
        }
    }
}
