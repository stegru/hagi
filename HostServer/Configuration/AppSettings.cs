namespace HostServer.Configuration
{
    public class AppSettings
    {
        public const string SectionName = "hagi";

        public string UserConfigPath { get; set; } = "hagi";

        public AppSettings Initialise()
        {
            return this;
        }
    }
}