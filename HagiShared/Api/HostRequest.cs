namespace HagiShared.Api
{
    public class HostRequest
    {
        public const string RootPath = "/hagi/";

        [Option("guest")]
        public string? Guest { get; set; }
    }

}