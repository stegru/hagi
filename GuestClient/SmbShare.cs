namespace Hagi.HagiGuest
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// SMB Network shares
    /// </summary>
    public static class SmbShare
    {
        // ReSharper disable StringLiteralTypo
        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetGetJoinInformation(IntPtr server, out IntPtr nameBuffer, out IntPtr bufferType);

        [DllImport("Netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr buffer);
        // ReSharper restore StringLiteralTypo

        /// <summary>
        /// Gets the machine name and the workgroup/domain (for identification on a windows network, not a DNS name)
        /// </summary>
        public static string FullName => SmbShare.fullName ??= SmbShare.GetFullName();

        private static string? fullName = null;

        /// <summary>Gets the name of this machine, and the workgroup/domain if appropriate.</summary>
        private static string GetFullName()
        {
            string? name;
            int result = SmbShare.NetGetJoinInformation(IntPtr.Zero, out IntPtr nameBuffer, out IntPtr _);

            if (result == 0 && nameBuffer != IntPtr.Zero)
            {
                name = Marshal.PtrToStringAuto(nameBuffer);
                _ = SmbShare.NetApiBufferFree(nameBuffer);
            }
            else
            {
                name = null;
            }

            return name == null || name.ToUpperInvariant() == "WORKGROUP"
                ? Environment.MachineName
                : $"{Environment.MachineName};{name}";
        }

        /// <summary>Check if the share exists.</summary>
        public static bool Check(string shareName)
        {
            return SmbShare.RunCommand($"Get-SmbShare -Name {shareName}") == 0;
        }

        /// <summary>Share the root directory.</summary>
        public static bool Add(string shareName)
        {
            if (SmbShare.Check(shareName))
            {
                return true;
            }

            return SmbShare.RunCommand($"New-SmbShare -Name {shareName} -Path C:\\", true) == 0;

        }

        /// <summary>Stop sharing the root directory.</summary>
        public static bool Remove(string shareName)
        {
            if (SmbShare.Check(shareName))
            {
                return SmbShare.RunCommand($"Remove-SmbShare -Name {shareName}", true) == 0;
            }

            return true;
        }

        private static int RunCommand(string command, bool elevated = false)
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