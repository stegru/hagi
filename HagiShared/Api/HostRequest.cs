namespace HagiShared.Api
{
    public class HostRequest
    {
        public const string RootPath = "/hagi/";

        [Option("guest", Info = "The guest ID.")]
        public virtual string Guest { get; set; } = null!;
    }

}