namespace HagiShared.Api
{
    public class HostResponse
    {
        public HostResponse(bool failed = false)
        {
            this.Failed = failed;
        }

        public bool Failed { get; set; }
    }
}