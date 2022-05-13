namespace App.Data
{
    public class LoggedInUserAccount : DecodedAccessToken
    {
        public bool IsAdmin => HasRole("Admin");
        public bool IsStaff => HasRole("Staff");
        public bool IsStudent => HasRole("Student");
        public string Firstname => GivenName;
        public string Surname => FamilyName;

        // public string Username => Username;
    }

}
