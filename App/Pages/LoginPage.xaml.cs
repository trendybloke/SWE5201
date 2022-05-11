using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using App.Data;
using App.Interfaces;
using App.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static App.Data.DecodedAccessToken;

namespace App
{
    public sealed partial class LoginPage : Page, IHasDataService, INotifyPropertyChanged
    {
        /* Testing user collections
        readonly string[] mockUsernames = { "Username" };
        readonly string[] mockPasshashes = { "Password".GetHashCode().ToString() };
        */

        public DataService dataService
        {
            get 
            {
                return ((App)App.Current)
                  .Container
                  .GetService<DataService>();
            }
        }

        private LoginUser loginUser;
        public LoginUser LoginUser
        {
            get
            {
                if(loginUser == null)
                {
                    loginUser = new LoginUser();
                }
                return this.loginUser;
            }
            set
            {
                this.loginUser = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoginUser)));
            }
        }

        private Action PostLoginHandler { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public LoginPage SetPostLoginAction(Action handler)
        {
            this.PostLoginHandler = handler;
            return this;
        }

        public LoginPage()
        {
            this.InitializeComponent();
        }

        // Toggles 'Log in' button to active if both input fields are filled.
        private void LoginButtonToggle(UIElement sender, CharacterReceivedRoutedEventArgs args)
        {
            if(this.UsernameTextBox.Text == String.Empty 
                || this.PasswordTextBox.Password == String.Empty)
            {
                LoginButton.IsEnabled = false;
            }
            else 
            {
                LoginButton.IsEnabled = true;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            /* Part of old mock dataa
            string username = this.UsernameTextBox.Text;
            string passHash = this.PasswordTextBox.Password.GetHashCode().ToString();
            */

            ContentDialog wrongUsername = new ContentDialog()
            {
                Title = "Invalid Login",
                Content = "User does not exist",
                CloseButtonText = "Ok"
            };

            ContentDialog wrongPassword = new ContentDialog()
            {
                Title = "Invalid Login",
                Content = "Incorrect password",
                CloseButtonText = "Ok"
            };

            /* Part of old mock data
            // Query API for valid Username
            if (mockUsernames.Contains(username))
            {
                // Query API for correct password
                int i;
                for(i = 0; i < mockUsernames.Length; i++)
                {
                    if (mockUsernames[i] == username)
                        break;
                }

                if(mockPasshashes[i] == passHash)
                {
                    this.Frame.Navigate(typeof(BrowsePage));
                }
                // Password is incorrect
                else
                {
                    await wrongPassword.ShowAsync();
                }

            }
            // User does not exist
            else
            {
                await wrongUsername.ShowAsync();
            }
            */

            try
            {
                var loginResponse = await dataService
                                            .RemoveBearerToken()
                                            .DisableLoginOnUnauthorized()
                                            .InsertAsync<LoginUser, Token>
                                                (LoginUser, "Login");

                // On login response
                if(loginResponse != null)
                {
                    dataService.LoggedInUserAccount = 
                        Decode<LoggedInUserAccount>(loginResponse);
                }

                // On OK login
                if(dataService.LoggedInUserAccount?.Username == LoginUser.Username)
                {
                    this.Frame.Navigate(typeof(BrowsePage));
                }
                // On Not OK login - Wrong Username
                else
                {
                    await wrongUsername.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog()
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "Ok"
                };

                await errorDialog.ShowAsync();
            }
        }
    }
}
