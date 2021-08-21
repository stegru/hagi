using System;
using HagiShared.Api;

namespace GuestClient
{
    public abstract class RequestOptions
    {
        [CommandLine.Option("host")]
        public string? Host { get; set; }

        [CommandLine.Option("config")]
        public string? ConfigFile { get; set; }

        public abstract string RequestUrl { get; }

        public abstract HostRequest GetRequest();

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