using System.Text.Json.Serialization;

namespace HagiShared.Api
{
    [Request("open", Info = "Opens a file on the host.")]
    public class OpenRequest : HostRequest
    {
        [Option("path", Info = "A url or a path on the guest.", Required = true, IsPayload = true)]
        [JsonPropertyName("path")]
        public string Path { get; set; } = null!;

        [Option("file", Info = "'path' is a file")]
        public bool File { get; set; }

        [Option("url", Info = "'path' is a url")]
        public bool Url { get; set; }
    }

    public class OpenResponse : HostResponse
    {
        public OpenResponse() : base()
        {
        }
    }
}