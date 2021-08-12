namespace Hagi.HagiGuest.Install
{
    using System;
    using System.Runtime.Versioning;
    using HagiShared.Platform;

    /// <summary>
    /// Add a shell command to all files.
    /// </summary>
    [Installer(Platform.Windows)]
    [SupportedOSPlatform("windows")]
    public class ShareInstaller : Installer
    {
        private const string ShareName = "hagi";

        public override void Install(InstallOptions options)
        {
            if (!SmbShare.Add(ShareInstaller.ShareName))
            {
                Console.WriteLine("Failed to create network share");
            }
        }

        public override void Remove(UninstallOptions options)
        {
            if (!SmbShare.Remove(ShareInstaller.ShareName))
            {
                Console.WriteLine("Failed to remove network share");
            }
        }

    }
}