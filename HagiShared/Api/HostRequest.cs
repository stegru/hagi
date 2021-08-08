using CommandLine;

namespace HagiShared.Api
{
    public class HostRequest : IRequestOptions
    {
        string? IRequestOptions.Host { get; set; }
    }

    public interface IRequestOptions
    {
        [Option("host")]
        public string? Host { get; set; }
    }

}