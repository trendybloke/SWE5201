using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using App.Data;
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
using Microsoft.Extensions.DependencyInjection;

namespace App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddStaffOrAdminView : Page
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

        public AddStaffOrAdminView()
        {
            this.InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Check each input field, make sure they are filled
            foreach (UIElement element in this.PageGrid.Children)
            {
                bool elementIsTextBox = element.GetType().Name == nameof(TextBox);
                bool elementIsPasswordBox = element.GetType() == typeof(PasswordBox);

                bool elementHasContent;

                ContentDialog emptyInput = new ContentDialog()
                {
                    Title = "Error: ncomplete form",
                    CloseButtonText = "Ok"
                };

                if (elementIsTextBox)
                {
                    TextBox tb = (TextBox)element;

                    elementHasContent = !string.IsNullOrEmpty(tb.Text);

                    // If this element is a text box that does not have text in, 
                    // display error content and return.
                    if (elementIsTextBox && !elementHasContent)
                    {
                        emptyInput.Content =
                            $"{tb.Header} field is empty. Please fill it.";

                        await emptyInput.ShowAsync();

                        return;
                    }
                }
                else if (elementIsPasswordBox)
                {
                    PasswordBox tb = (PasswordBox)element;

                    elementHasContent = !string.IsNullOrEmpty(tb.Password);

                    if (elementIsPasswordBox && !elementHasContent)
                    {
                        emptyInput.Content =
                            $"{tb.Header} field is empty. Please fill it.";

                        await emptyInput.ShowAsync();

                        return;
                    }
                }
            }
            // Reaching here means all fields are filled.

            // Check email is a valid email
            string inpEmail = EmailTextBox.Text;

            if (!(inpEmail.Contains('@') || inpEmail.Contains('.')))
            {
                ContentDialog invalidEmail = new ContentDialog()
                {
                    Title = "Invalid Email",
                    Content = "Please make sure you entered a real email address.",
                    CloseButtonText = "Ok"
                };

                await invalidEmail.ShowAsync();
                return;
            }

            // Check passwords match
            string pass = PasswordTextBox.Password;
            string confPass = ConfirmPasswordTextBox.Password;

            if (pass != confPass)
            {
                ContentDialog nonMatchPass = new ContentDialog()
                {
                    Title = "Passwords do not match",
                    Content = "PLease ensure you enterred the correct password in both fields.",
                    CloseButtonText = "Ok"
                };

                await nonMatchPass.ShowAsync();
                return;
            }

            // Insert new user
            bool isAdmin = AdminCheckBox.IsChecked == true;
            bool isStaff = StaffCheckBox.IsChecked == true;

            EditableUser newUser = new EditableUser()
            {
                Admin = isAdmin,
                Staff = isStaff,
                Student = false,
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                Email = inpEmail,
                Password = pass,
                PasswordConfirmation = confPass
            };

            var registerResult = (await dataService.InsertAsync<EditableUser>(newUser, "Register"));

            // Alert if failed
            if (!dataService.LastResponse.IsSuccessStatusCode)
            {
                ContentDialog failedToRegister = new ContentDialog()
                {
                    Title = "Failed to register new user",
                    Content = $"Error code: {dataService.LastResponse.StatusCode}",
                    CloseButtonText = "Ok"
                };

                await failedToRegister.ShowAsync();
                return;
            }
            // Navigate to login if successful
            ContentDialog registerSuccess = new ContentDialog()
            {
                Title = "New User registered",
                Content = "Succesfully created user",
                CloseButtonText = "Ok"
            };

            await registerSuccess.ShowAsync();
        }
    }
}
