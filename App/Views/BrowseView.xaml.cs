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
        private DataService dataService
        {
            get
            {
                return ((App)App.Current)
                            .Container
                            .GetService<DataService>();
            }
        }

        List<HostedEvent> HostedEvents;

        public event PropertyChangedEventHandler PropertyChanged;

        public BrowseView()
        {
            this.RefreshEventList();
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //this.RefreshEventList();
        }

        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshContent(GetSelectedEvent());
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
