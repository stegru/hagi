namespace Hagi.Shared.Api.Config
{
    public class PasswordRequest : ConfigRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string NewUsername { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}