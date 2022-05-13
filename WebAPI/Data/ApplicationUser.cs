using Microsoft.AspNetCore.Identity;
namespace WebAPI.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
