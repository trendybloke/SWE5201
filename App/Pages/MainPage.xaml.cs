using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace App.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
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

                if(ContentFrame.Navigate(page, null, new EntranceNavigationTransitionInfo()))
                {
                    _lastItem = item;
                    nvMain.Header = item.Content.ToString();
                }
                
            }
        }
    }
}
