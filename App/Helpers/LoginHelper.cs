using App.Data;
using App.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace App.Helpers
{
    public static class LoginHelper
    {
        public static void Login(this Page page)
        {
            NavigatedEventHandler navigatedHandler = null;
            navigatedHandler = (object sender, NavigationEventArgs args) =>
            {
                var userLoginPage = args.Content as LoginPage;
                if (userLoginPage != null)
                {
                    userLoginPage.LoginUser = new LoginUser()
                    {
                        // Can be used to pre-fill login details
                        Email = "admin@events.bolton.ac.uk",
                        Password = "Super$ecretPassw0rd"
                    };
                    userLoginPage.SetPostLoginAction(() =>
                    {
                        System.Diagnostics.Debug.WriteLine("Logged in!");
                        var mainPage = (BrowsePage)((ContentControl)Window.Current.Content).Content;
                        var currentDataServiceEnabledPage = page as IHasDataService;
                        if (currentDataServiceEnabledPage.dataService.LoggedInUserAccount != null &&
                         currentDataServiceEnabledPage.dataService.LoggedInUserAccount.IsAdmin)
                        {
                            mainPage.ManageUsersVisibility = Visibility.Visible;
                        }
                        else
                        {
                            mainPage.ManageUsersVisibility = Visibility.Collapsed;
                        }
                    });
                }
                page.Frame.Navigated -= navigatedHandler;
            };
            page.Frame.Navigated += navigatedHandler;
            page.Frame.Navigate(typeof(LoginPage));
        }
        public static void Logout(this Page page)
        {
            var currentDataServiceEnabledPage = page as IHasDataService;
            currentDataServiceEnabledPage.dataService.RemoveBearerToken();
            currentDataServiceEnabledPage.dataService.LoggedInUserAccount = null;
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(BrowsePage));
        }

    }
}
