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
using App.Data;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowseHostedEventsView : Page, INotifyPropertyChanged
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

        private BrowseViewState browseViewState;

        public event PropertyChangedEventHandler PropertyChanged;

        public BrowseHostedEventsView()
        {
            this.RefreshEventList("", "");
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

        private async void RefreshEventList(string titleFilter, string tagsFilter)
        {
            
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
            
            if (browseViewState == BrowseViewState.UPCOMING_EVENTS)
                allEvents = allEvents.Where(x => x.StartTime.CompareTo(DateTime.Now) == 1).ToList<HostedEvent>();

            if (browseViewState == BrowseViewState.BOOKED_EVENTS)
            {
                var usersBookings = (await dataService.GetAllAsync<EventBooking>
                         ("ByUser", new List<string> { dataService.LoggedInUserAccount.Id }))
                                         .Select(x => new EventBooking
                                         {
                                             Id = x.Id,
                                             UserId = x.UserId,
                                             HostedEventId = x.HostedEventId,
                                             //HostedEvent = await dataService.GetAsync<HostedEvent, int>(x.HostedEventId),
                                             Attended = x.Attended,
                                         }).ToList();

                    // Need to determine if a given userBooking.HostedEventId appears in allEvents
                    // If it doesn't, then that hosted event needs to be removed from allEvents

                List<HostedEvent> bookedEvents = new List<HostedEvent>();

                foreach (var booking in usersBookings)
                {
                    booking.HostedEvent = await dataService.GetAsync<HostedEvent, int>(booking.HostedEventId);

                    bookedEvents.Add(booking.HostedEvent);
                }

                allEvents = bookedEvents;
            }

            if (!string.IsNullOrEmpty(titleFilter))
                allEvents = allEvents.Where(x => x.Event.Title.Contains(titleFilter)).ToList<HostedEvent>();

            if (!string.IsNullOrEmpty(tagsFilter))
            {
                string[] tags;

                if (tagsFilter.Contains(", "))
                    tags = tagsFilter.Split(", ");
                else
                    tags = new string[] { tagsFilter };

                foreach(var tag in tags)
                {
                    allEvents = allEvents.Where(x => x.Event.Tags.Any(t => t.Content.Contains(tag))).ToList<HostedEvent>();
                }
            }

            if(DateFilterComboBox != null)
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


        private void RefreshContent(HostedEvent viewedEvent)
        {
            // Populate Content page
            TitleTextBlock.Text = viewedEvent.Event.Title;
            RoomTextBlock.Text = $"Room: {viewedEvent.Room.Name}";
            TagsTextBlock.Text = $"Tags: {viewedEvent.Event.TagsString}";

            DateTime start = viewedEvent.StartTime;
            DateTime end = viewedEvent.EndTime;

            // Disable book button if event has started
            BookButton.IsEnabled = start.CompareTo(DateTime.Now) == 1;

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
            this.RefreshEventList("", "");
            //RefreshContent(GetSelectedEvent());
            EventList.ItemsSource = HostedEvents;
            EventList.SelectedIndex = 0;
            if (dataService.LoggedInUserAccount.Role.Contains("Student") &&
                browseViewState != BrowseViewState.BOOKED_EVENTS)
                BookButton.Visibility = Visibility.Visible;
        }

        private void EventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(HostedEvents != null && EventList.SelectedIndex >= 0)
            {
                RefreshContent(GetSelectedEvent());
                _lastEvent = GetSelectedEvent();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string[] wantedEvents = (e.Parameter as NavigationViewItem).Content.ToString().Split(" ");

            bool clickedUpcoming =
                wantedEvents[1] == "Upcoming";

            if(clickedUpcoming)
            {
                browseViewState = BrowseViewState.UPCOMING_EVENTS;
                return;
            }

            bool clickedBooked =
                wantedEvents[0] == "Booked";

            if (clickedBooked)
            {
                browseViewState = BrowseViewState.BOOKED_EVENTS;
                return;
            }

            browseViewState = BrowseViewState.ALL_EVENTS;
        }

        private async void BookButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected event
            HostedEvent selectedEvent = _lastEvent;

            // Confimation dialog
            ContentDialog confirmBook = new ContentDialog()
            {
                Title = "Confirm booking?",
                Content = $"Room {selectedEvent.Room.Name} " +
                $"at {selectedEvent.StartTime.ToString("HH:mm")} " +
                $"on {selectedEvent.StartTime.ToString("dddd, dd/MM/yyyy")}",
                PrimaryButtonText = "Confirm",
                CloseButtonText = "Cancel"
            };


            // Show confirm dialog
            var userAction = await confirmBook.ShowAsync();

            // If Primary Button selected
            if (userAction == ContentDialogResult.Primary)
            {
                // Already booked dialog
                ContentDialog alreadyBooked = new ContentDialog()
                {
                    Title = "Unable to book",
                    Content = "You're already booked to see this event!",
                    CloseButtonText = "Ok"
                };

                // Get this user
                //UserSummary thisUser =
                //    await dataService
                //            .GetAsync<UserSummary, string>
                //                (dataService.LoggedInUserAccount.Id, "Summary");

                // Book event
                //EventBooking newBooking = new EventBooking()
                //{
                //    HostedEvent = _lastEvent,
                //    User = new UserSummary {
                //        Id = dataService.LoggedInUserAccount.Id,
                //        Email = dataService.LoggedInUserAccount.Email,
                //        FirstName = dataService.LoggedInUserAccount.Firstname,
                //        LastName = dataService.LoggedInUserAccount.Surname
                //    },
                //    Attended = false
                //};

                EventBooking newBooking = new EventBooking()
                {
                    HostedEventId = _lastEvent.Id,
                    UserId = dataService.LoggedInUserAccount.Id,
                    Attended = false
                };

                //if (dataService.GetAllAsync<EventBooking>
                //    ("ByUser", 
                //        new List<string>() 
                //        { dataService.LoggedInUserAccount.Id }
                //    ).Result.Contains(newBooking))
                //{
                //    await alreadyBooked.ShowAsync();
                //    return;
                //}

                var usersBookings = (await dataService.GetAllAsync<EventBooking>
                        ("ByUser", new List<string> { dataService.LoggedInUserAccount.Id }))
                                        .Select(x => new EventBooking
                                        {
                                            Id = x.Id,
                                            UserId = x.UserId,
                                            HostedEventId = x.HostedEventId,
                                            Attended = x.Attended,
                                        }).ToList();

                if(usersBookings.Any(x => x.HostedEventId == newBooking.HostedEventId))
                {
                    await alreadyBooked.ShowAsync();
                    return;
                }


                var bookResult = 
                    await dataService.InsertAsync<EventBooking>(newBooking, "Book");

                if(bookResult == null)
                {
                    ContentDialog bookFailed = new ContentDialog()
                    {
                        Title = "Failed to book onto Event",
                        Content = dataService.LastResponse,
                        CloseButtonText = "Ok"
                    };

                    await bookFailed.ShowAsync();
                    return;
                }

                ContentDialog bookSucceeded = new ContentDialog()
                {
                    Title = "Success",
                    Content = "Booked Event",
                    CloseButtonText = "Ok"
                };

                await bookSucceeded.ShowAsync();
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

            if(EventList != null)
                EventList.ItemsSource = HostedEvents;
        }
    }
}
