using CommandLine;

namespace HagiShared.Api
{
    public class HostRequest : IRequestOptions
    {
        [Option("guest")]
        public string Guest { get; set; }

        string? IRequestOptions.Host { get; set; }
    }

    public interface IRequestOptions
    {
        [Option("host")]
        public string? Host { get; set; }
    }

}