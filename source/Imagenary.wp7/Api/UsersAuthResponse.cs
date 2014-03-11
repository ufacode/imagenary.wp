namespace Imagenary.Api
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }

    public class UsersAuthResponse
    {
        public User User { get; set; }
        public Status Status { get; set; }
    }
}