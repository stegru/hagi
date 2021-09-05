namespace Hagi.HagiGuest.Install
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Versioning;
    using System.Text;
    using HagiShared.Platform;
    using Microsoft.Win32;

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
            if (!this.CheckShare())
            {
                int result = this.RunCommand($"New-SmbShare -Name {ShareInstaller.ShareName} -Path C:\\", true);
                if (result != 0)
                {
                    Console.WriteLine("Failed to create network share");
                }
            }
        }

        public override void Remove(UninstallOptions options)
        {
            if (this.CheckShare())
            {
                int result = this.RunCommand($"Remove-SmbShare -Name {ShareInstaller.ShareName}", true);
                if (result != 0)
                {
                    Console.WriteLine("Failed to remove network share");
                }
            }
        }

        /// <summary>Check if the share exists.</summary>
        public bool CheckShare()
        {
            return this.RunCommand($"Get-SmbShare -Name {ShareInstaller.ShareName}") == 0;
        }

        private int RunCommand(string command, bool elevated = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("powershell.exe")
            {
                UseShellExecute = true,
                Verb = elevated ? "runas" : "",
                WindowStyle = ProcessWindowStyle.Minimized
            };

            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");

            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");

            startInfo.ArgumentList.Add("-EncodedCommand");
            startInfo.ArgumentList.Add(Convert.ToBase64String(Encoding.Unicode.GetBytes(command)));

            using Process process = new Process() { StartInfo = startInfo };

            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception)
            {
                return -1;
            }

            return process.ExitCode;
        }
    }
}