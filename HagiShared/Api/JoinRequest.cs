namespace HagiShared.Api
{
    [Request("auth/join")]
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