using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using App.Services;
using App.Models;
using App.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace App.Views
{
    public sealed partial class AttendanceView : Page
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

        private List<HostedEvent> HostedEvents;

        private HostedEvent _lastEvent;

        public AttendanceView()
        {
            this.RefreshEventList();
            this.InitializeComponent();
        }

        private HostedEvent GetSelectedEvent()
        {
            return HostedEvents.ElementAt(EventList.SelectedIndex);
        }

        private async void RefreshEventList(string titleFilter = null, string tagsFilter = null)
        {
            // Get all upcoming events
            var allEvents = 
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
                })
                .OrderBy(e => e.StartTime)
                .ToList();

            allEvents = allEvents
                            .Where(x => x.StartTime.CompareTo(DateTime.Now) == 1)
                            .ToList<HostedEvent>();

            // Apply filters 
            if (!string.IsNullOrEmpty(titleFilter))
                allEvents = allEvents.Where(x => x.Event.Title.Contains(titleFilter)).ToList<HostedEvent>();

            if (!string.IsNullOrEmpty(tagsFilter))
            {
                string[] tags;

                if (tagsFilter.Contains(", "))
                    tags = tagsFilter.Split(", ");
                else
                    tags = new string[] { tagsFilter };

                foreach (var tag in tags)
                {
                    allEvents = allEvents.Where(x => x.Event.Tags.Any(t => t.Content.Contains(tag))).ToList<HostedEvent>();
                }
            }

            if (DateFilterComboBox != null)
            {
                switch (DateFilterComboBox.SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        allEvents = allEvents
                                        .Where(x => EventOccursThisWeek(x)).ToList();
                        break;
                    case 2:
                        allEvents = allEvents
                                        .Where(x => EventOccursNextWeek(x)).ToList();
                        break;
                    case 3:
                        allEvents = allEvents
                                        .Where(x => EventOccursThisMonth(x)).ToList();
                        break;
                }
            }

            HostedEvents = allEvents;
            if (EventList != null)
                EventList.ItemsSource = HostedEvents;
        }

        private async void RefreshContent()
        {
            if (_lastEvent == null)
                return;

            // Get bookings related to _lastEvent where attendance is false
            var bookings = (await dataService.GetAllAsync<EventBooking>
                    ("ByHostedEvent", new List<string> { Convert.ToString(_lastEvent.Id) }))
                    .Select(b => new EventBooking
                    {   
                        Id = b.Id,
                        UserId = b.UserId,
                        HostedEventId = b.HostedEventId,
                        Attended = b.Attended
                    })
                    .Where(b => b.Attended == false)
                    //.Where(b => b.HostedEventId == _lastEvent.Id);
                    .ToList();

            if (bookings == null)
                return;

            // Get users from each booking 
            List<UserSummary> missingUsers = new List<UserSummary>();
                    
            foreach(var booking in bookings)
            {
                var user = await dataService
                                        .GetAsync<UserSummary, string>
                                            (booking.UserId, "Summary");

                missingUsers.Add(user);
            }

            // Set UserList item source to those users
            UserList.ItemsSource = missingUsers;
        }

        private bool EventOccursThisWeek(HostedEvent _event)
        {
            int thisWeek = DateTime.Now.DayOfYear - (int)DateTime.Now.DayOfWeek + 1;
            return thisWeek < _event.StartTime.DayOfYear
                && thisWeek + 7 > _event.EndTime.DayOfYear;
        }

        private bool EventOccursNextWeek(HostedEvent _event)
        {
            int nextWeek = (DateTime.Now.DayOfYear + 7) - (int)DateTime.Now.DayOfWeek + 1;
            return nextWeek < _event.StartTime.DayOfYear
                && nextWeek + 7 > _event.EndTime.DayOfYear;
        }

        private bool EventOccursThisMonth(HostedEvent _event)
        {
            return DateTime.Now.Month == _event.StartTime.Month;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshEventList("", "");
            EventList.ItemsSource = HostedEvents;
            EventList.SelectedIndex = 0;
            _lastEvent = GetSelectedEvent();
        }

        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HostedEvents != null && EventList.SelectedIndex >= 0)
            {
                _lastEvent = GetSelectedEvent();
                RefreshContent();
            }
        }

        private void TitleFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.RefreshEventList(TitleFilterTextBox.Text, TagFilterTextBox.Text);
            EventList.ItemsSource = HostedEvents;
        }

        private void TagFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.RefreshEventList(TitleFilterTextBox.Text, TagFilterTextBox.Text);
            EventList.ItemsSource = HostedEvents;
        }

        private void DateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RefreshEventList(TitleFilterTextBox.Text, TagFilterTextBox.Text);

            if (EventList != null)
                EventList.ItemsSource = HostedEvents;
        }

        private async void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Get the User this box refers to
            CheckBox cb = (CheckBox)e.OriginalSource;

            string username = cb.Tag.ToString();

            // Get the booking in this context
            var user = (await dataService.GetAllAsync<UserSummary>("List"))
                .Where(u => u.Name == username)
                .FirstOrDefault();

            EventBooking booking = (await dataService.GetAllAsync<EventBooking>("ByUser", new List<string> { user.Id }))
                                                    .Where(b => b.HostedEventId == _lastEvent.Id)
                                                    .FirstOrDefault();

            // Update attendance
            booking.Attended = true;

            var result = await dataService.UpdateAsync<EventBooking, int>(booking, booking.Id);

            if (result)
            {
                // Refresh page
                RefreshContent();
            }
            else
            {
                ContentDialog err = new ContentDialog()
                {
                    Title = "Could not attend user",
                    Content = dataService.LastResponse,
                    CloseButtonText = "Ok"
                };

                await err.ShowAsync();
            }
        }
    }
}
