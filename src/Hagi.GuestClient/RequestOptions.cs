namespace Hagi.HagiGuest
{
    using System;
    using System.IO;
    using System.Linq;
    using Shared.Api;

    public abstract class ClientOptions
    {
        private static readonly Type[] Types = { typeof(InstallOptions), typeof(UninstallOptions) };
        public static readonly Type[] AllTypes = ClientOptions.Types.Concat(RequestOptions.GeneratedTypes).ToArray();

        public virtual void ApplyDefaults()
        {
        }
    }

    public abstract partial class RequestOptions : ClientOptions
    {
        [CommandLine.Option("host", HelpText = "The address of the host, if auto-detection does not work.", MetaValue = "<host>")]
        public string? Host { get; set; }

        public string GuestId { get; set; } = string.Empty;

        [CommandLine.Option("secret", Hidden = true)]
        public string? Secret { get; set; }

        public abstract string RequestUrl { get; }

        public abstract HostRequest GetRequest();
    }

    [CommandLine.Verb("install", HelpText = "Installs the guest client onto this machine")]
    public class InstallOptions : ClientOptions
    {
        public string InstallDir { get; set; } = Config.GetConfigFilePath();
        public string ExePath { get; set; } = null!;

        public string ExeName => Path.GetFileName(this.ExePath);
    }

    [CommandLine.Verb("uninstall", HelpText = "Uninstalls the guest client from this machine")]
    public class UninstallOptions : InstallOptions
    {
    }

    public partial class JoinRequestOptions
    {
        public override void ApplyDefaults()
        {
            base.ApplyDefaults();
            this.MachineName ??= SmbShare.FullName;
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