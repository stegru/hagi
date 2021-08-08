using CommandLine;

namespace HagiShared.Api
{
    [Verb("map")]
    public class FileMapRequest : HostRequest
    {
        [Value(0)]
        public string Path { get; set; } = null!;
    }

    public class FileMapResponse : HostResponse
    {
        public FileMapResponse(string? path) : base(string.IsNullOrEmpty(path))
        {
            this.Path = path ?? string.Empty;
        }

        public string Path { get; set; }

    }
}