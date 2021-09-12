namespace Hagi.Shared.Api.Config
{
    public class LoginRequest : ConfigRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponse : ConfigRequest
    {
        public string Token { get; set; }

        public LoginResponse(string token)
        {
            this.Token = token;
        }
    }
}