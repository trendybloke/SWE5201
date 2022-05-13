using System.ComponentModel.DataAnnotations;

namespace WebAPI.ViewModels
{
    public class LoginUserViewModel
    {
        [Required(ErrorMessage = "User Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
