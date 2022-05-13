using System.ComponentModel.DataAnnotations;

namespace WebAPI.ViewModels
{
    public class EditableUserViewModel
    {
        public string? Id { get; set; }
        public bool Admin { get; set; }
        public bool Staff { get; set; }
        public bool Student { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public string? Password { get; set; }

        [Compare("Password")]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^azA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", 
            ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 ofthe following: upper case (A - Z), lower case (a - z), number(0 - 9) and special character(e.g. !@#$%^&*)")]
        public string? PasswordConfirmation { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
