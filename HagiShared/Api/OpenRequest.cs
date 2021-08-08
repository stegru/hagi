using System.Text.Json.Serialization;
using CommandLine;

namespace HagiShared.Api
{
    [Verb("open", HelpText = "Opens a file or url on the host")]
    public class OpenRequest : HostRequest
    {
        [Value(0)]
        [JsonPropertyName("path")]
        public string Path { get; set; } = null!;

        [Option('t', "type")]
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