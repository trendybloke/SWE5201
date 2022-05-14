using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using App.Services;
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
using System.Threading.Tasks;

namespace App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewUsersView : Page
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

        private List<EditableUser> Users;

        private EditableUser _lastUser;

        public ViewUsersView()
        {
            //this.RefreshUserList();
            this.InitializeComponent();
        }

        private EditableUser GetSelectedUser()
        {
            return Users.ElementAt(UserList.SelectedIndex);
        }

        private async Task<bool> RefreshUserList(string nameFilter = null, string rolesFilter = null)
        {
            // Get summaries of all users.
            var allUserSummaries =
                (await dataService.GetAllAsync<UserSummary>("List"))
                    .Select(x => new UserSummary
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                    }).ToList();

            List<EditableUser> allUsers = new List<EditableUser>();

            // Create allUsers by querying each user and getting the rest of their details
            // (GROSS AND ALSO SLOW)
            foreach (var user in allUserSummaries)
                allUsers.Add(await dataService.GetAsync<EditableUser, string>(user.Id, "Modify"));

            if (!string.IsNullOrEmpty(nameFilter))
                allUsers = allUsers
                                .Where(x => x.Name.Contains(nameFilter))
                                .ToList();

            if (!string.IsNullOrEmpty(rolesFilter))
            {
                string[] roles;
                if (rolesFilter.Contains(", "))
                    roles = rolesFilter.Split(", ");
                else
                    roles = new string[] { rolesFilter };

                foreach(var role in roles)
                {
                    allUsers = allUsers
                                .Where(x => GetUserRoleString(x).Contains(role))
                                .ToList();
                }
            }

            Users = allUsers;

            return true;
        }
        private string GetUserRoleString(EditableUser user)
        {
            StringBuilder roles = new StringBuilder();

            if (user.Student)
            {
                roles.Append("Student");
                if (user.Staff || user.Admin)
                    roles.Append(", ");
            }

            if (user.Staff)
            {
                roles.Append("Staff");
                if (user.Admin)
                    roles.Append(", ");
            }

            if (user.Admin)
                roles.Append("Admin");

            return roles.ToString();
        }

        private void RefreshContent(EditableUser user)
        {
            // Fill input fields with this user
            FirstNameTextBox.Text = user.FirstName;
            LastNameTextBox.Text = user.LastName;
            EmailTextBox.Text = user.Email;
            //PasswordTextBox.Password = user.Password;
            //ConfirmPasswordTextBox.Password = user.PasswordConfirmation;

            StudentCheckBox.IsChecked = user.Student;
            StaffCheckBox.IsChecked = user.Staff;
            AdminCheckBox.IsChecked = user.Admin;

            // Enable Update button
            _lastUser = user;
            UpdateButton.IsEnabled = true;
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await this.RefreshUserList();
            UserList.ItemsSource = Users;
        }
        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Users != null && UserList.SelectedIndex >= 0)
                RefreshContent(GetSelectedUser());
        }
        private void NameFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshUserList(NameFilterTextBox.Text, RolesFilterTextBox.Text);
            UserList.ItemsSource = Users;
        }
        private void RolesFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshUserList(NameFilterTextBox.Text, RolesFilterTextBox.Text);
            UserList.ItemsSource = Users;
        }
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
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
                //else if (elementIsPasswordBox)
                //{
                //    PasswordBox tb = (PasswordBox)element;

                //    elementHasContent = !string.IsNullOrEmpty(tb.Password);

                //    if (elementIsPasswordBox && !elementHasContent)
                //    {
                //        emptyInput.Content =
                //            $"{tb.Header} field is empty. Please fill it.";

                //        await emptyInput.ShowAsync();

                //        return;
                //    }
                //}
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

            // Check passwords match if entered
            bool passwordsEntered = !string.IsNullOrEmpty(PasswordTextBox.Password);
            if (passwordsEntered)
            {
                string pass = PasswordTextBox.Password;
                string confPass = ConfirmPasswordTextBox.Password;

                if (pass != confPass)
                {
                    ContentDialog nonMatchPass = new ContentDialog()
                    {
                        Title = "Passwords do not match",
                        Content = "Please ensure you enterred the correct password in both fields.",
                        CloseButtonText = "Ok"
                    };

                    await nonMatchPass.ShowAsync();
                    return;
                }
            }

            // Insert new user
            bool isAdmin = AdminCheckBox.IsChecked == true;
            bool isStaff = StaffCheckBox.IsChecked == true;
            bool isStudent = StudentCheckBox.IsChecked == true;

            EditableUser newUser = new EditableUser()
            {
                Id = _lastUser.Id,
                Admin = isAdmin,
                Staff = isStaff,
                Student = isStudent,
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                Email = inpEmail,
                Password = passwordsEntered ? PasswordTextBox.Password: null,
                PasswordConfirmation = passwordsEntered ? ConfirmPasswordTextBox.Password : null
            };

            //var registerResult = (await dataService.InsertAsync<EditableUser>(newUser, "Register"));
            var updateResult = (await dataService.UpdateAsync<EditableUser, string>(newUser, newUser.Id, "Modify"));

            // Alert if failed
            if (!dataService.LastResponse.IsSuccessStatusCode)
            {
                ContentDialog failedToUpdate = new ContentDialog()
                {
                    Title = "Failed to update user",
                    Content = $"Error code: {dataService.LastResponse.StatusCode}",
                    CloseButtonText = "Ok"
                };

                await failedToUpdate.ShowAsync();
                return;
            }
            // Navigate to login if successful
            ContentDialog updateSuccess = new ContentDialog()
            {
                Title = "User updated",
                Content = "Succesfully updated user",
                CloseButtonText = "Ok"
            };

            var windowClosed = await updateSuccess.ShowAsync();

            if(windowClosed != null)
            {
                await this.RefreshUserList();

                _lastUser = newUser;

                this.RefreshContent(_lastUser);
            }
        }
    }
}
