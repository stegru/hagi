using System.Text.Json.Serialization;

namespace HagiShared.Api
{
    [Request("open", Info = "Opens a file on the host.")]
    public class OpenRequest : HostRequest
    {
        [Option("path", Info = "A url or a path on the guest.", Required = true, IsPayload = true)]
        [JsonPropertyName("path")]
        public string Path { get; set; } = null!;

        [Option("type")]
        public OpenPathType Type { get; set; }
    }

    public enum OpenPathType
    {
        Any = 0,
        File = 1,
        Url = 2
    }

    public class OpenResponse : HostResponse
    {
        public OpenResponse() : base()
        {
        }
    }
}