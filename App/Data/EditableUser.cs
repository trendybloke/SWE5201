namespace App.Data
{
    public class EditableUser
    {
        public string Id { get; set; }
        public bool Admin { get; set; }
        public bool Staff { get; set; }
        public bool Student { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public string Name
        {
            get
            {
                return $"{this.FirstName} {this.LastName}";
            }
        }
    }
}
