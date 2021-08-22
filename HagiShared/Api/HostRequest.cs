namespace HagiShared.Api
{
    public class HostRequest
    {
        public const string RootPath = "/hagi/";

        [Option("guest", Info = "The guest ID.")]
        public string? Guest { get; set; }
    }

}