namespace Hagi.Shared.Api.Config
{
    public class ConfigRequest : HostRequest
    {
    }

    public class ConfigResponse : HostResponse
    {
    }

    public class GetConfigResponse : ConfigResponse
    {
        public string Config { get; set; }
    }

    public class StartConfigResponse : ConfigResponse
    {
        public bool AccountCreated { get; set; }
        public bool InitialSetupComplete { get; }

        public StartConfigResponse(bool accountCreated, bool initialSetupComplete)
        {
            this.AccountCreated = accountCreated;
            this.InitialSetupComplete = initialSetupComplete;
        }

    }

}