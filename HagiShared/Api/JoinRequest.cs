namespace HagiShared.Api
{
    [Request("auth/join", Info = "Join the host.")]
    public class JoinRequest : HostRequest
    {
        [Option("secret")]
        public string SharedSecret { get; set; } = null!;
    }

    public class JoinResponse : HostResponse
    {
        public string GuestId { get; }

        public JoinResponse(string guestId)
        {
            this.GuestId = guestId;
        }

    }
}