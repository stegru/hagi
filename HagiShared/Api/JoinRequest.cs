namespace HagiShared.Api
{
    [Request("auth/join", Info = "Join the host.")]
    public class JoinRequest : HostRequest
    {
        [Option("guest", Info = "The guest ID.")]
        public virtual string Guest { get; set; } = null!;

        [Option("secret")]
        public string? SharedSecret { get; set; }
    }

    public class JoinResponse : HostResponse
    {
        public string GuestId { get; }
        public string GuestSecret { get; }

        public JoinResponse(string guestId, string guestSecret)
        {
            this.GuestId = guestId;
            this.GuestSecret = guestSecret;
        }

    }
}