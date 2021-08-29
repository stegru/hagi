using System;
using HagiShared.Api;

namespace GuestClient
{
    using System.Linq;

    public abstract partial class RequestOptions
    {
        private static Type[] types = { typeof(Install) };

        public static Type[] AllTypes = RequestOptions.types.Concat(RequestOptions.GeneratedTypes).ToArray();

        [CommandLine.Option("host", HelpText = "The address of the host, if auto-detection does not work.", MetaValue = "<host>")]
        public string? Host { get; set; }

        public string GuestId { get; set; } = String.Empty;

        [CommandLine.Option("secret", Hidden = true)]
        public string? Secret { get; set; }

        public abstract string RequestUrl { get; }


        public abstract HostRequest GetRequest();
    }

    [CommandLine.Verb("install", HelpText = "Installs the guest client onto this machine")]
    public class Install : RequestOptions
    {
        public override string RequestUrl => string.Empty;

        public override HostRequest GetRequest()
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class RequestPathAttribute : Attribute
    {
        public string Path { get; }

        public RequestPathAttribute(string path)
        {
            this.Path = path;
            // TODO: Implement code here
            throw new NotImplementedException();
        }
    }

}