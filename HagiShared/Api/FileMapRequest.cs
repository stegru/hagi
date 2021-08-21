namespace HagiShared.Api
{
    [Request("map")]
    public class FileMapRequest : HostRequest
    {
        [Option("path")]
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