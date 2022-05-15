using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Input;
using App.Services;
using App.Data;
using Microsoft.Extensions.DependencyInjection;

namespace App.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
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

        NavigationViewItem _lastItem;
        public NavigationView NavigationView
        {
            get { return nvMain; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            _lastItem = null;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.NavigateTo(this.nvMain.MenuItems[0] as NavigationViewItem);

            LoggedInUserAccount thisUser = dataService.LoggedInUserAccount;

            if (thisUser.IsStudent)
            {
                // Show favourited events
                FavouritedEventsNavItem.Visibility = Visibility.Visible;
            }

            if(thisUser.IsStaff || thisUser.IsAdmin)
            {
                // Show Host Events Header
                HostEventsNavHeader.Visibility = Visibility.Visible;

                // Show Host Events
                HostableEventsNavItem.Visibility = Visibility.Visible;

                // Show Create new Event
                CreateNewEventNavItem.Visibility = Visibility.Visible;

                // Show Attendance view
                AttendanceNavItem.Visibility = Visibility.Visible;
            }

            if (thisUser.IsAdmin)
            {
                // Show User Management Header
                UserManagementNavHeader.Visibility = Visibility.Visible;

                // Show User Management
                UserManagmentNavItem.Visibility = Visibility.Visible;

                // Show Add Staff or Admin
                AddStaffAdminNavItem.Visibility = Visibility.Visible;
            }
        }
        private void nvMain_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            this.NavigateTo(item);
        }

        private void NavigateTo(NavigationViewItem item)
        {
            if (item == null || item == _lastItem)
                return;

            var clickedView = item.Tag?.ToString();
            Type page = Assembly
                .GetExecutingAssembly()
                .GetType(clickedView);

            if(!string.IsNullOrWhiteSpace(clickedView) && page != null)
            {
                //ContentFrame.Content = page;
                //_lastItem = item;
                //nvMain.Header = item.Content.ToString();

                
                if(ContentFrame.Navigate(page, item, new EntranceNavigationTransitionInfo()))
                {
                    _lastItem = item;
                    nvMain.Header = item.Content.ToString();
                }
                

                //NavigationView.Content = page;
                //_lastItem = item;
                //nvMain.Header = item.Content.ToString();
            }
        }

        private async void LogoutNavItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ContentDialog confirmLogout = new ContentDialog()
            {
                Title = "Confirm logout",
                Content = "Are you sure you want to log out?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            var confirmation = await confirmLogout.ShowAsync();

            if(confirmation == ContentDialogResult.Primary)
            {
                dataService.LoggedInUserAccount = null;
                this.Frame.Navigate(typeof(LoginPage));
            }
        }
    }
}
