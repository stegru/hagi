namespace Hagi.HagiGuest.Install
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using HagiShared.Platform;
    using Microsoft.Win32;

    /// <summary>
    /// Registers the executable with Windows, and updates the PATH environment.
    /// </summary>
    [Installer(Platform.Windows)]
    [SupportedOSPlatform("windows")]
    internal class ExeInstaller : Installer
    {
        private RegistryHandler _registry = new RegistryHandler();

        private IEnumerable<RegistryHandler.RegistryItem> GetRegistryItems(InstallOptions options)
        {
            return new[]
            {
                // Add to "App Paths"
                new RegistryHandler.RegistryItem(@"Software\Microsoft\Windows\CurrentVersion\App Paths\hagi-guest", "",
                    options.ExePath),
                new RegistryHandler.RegistryItem(@"Software\Classes\HagiGuest.open", "", "Open on host"),
                new RegistryHandler.RegistryItem(@"Software\Classes\HagiGuest.open\shell\open\command", "",
                    $"\"{options.ExePath}\" open --url \"%1\""),
            };
        }

        private void GetPaths(InstallOptions options)
        {
            string currentExe = Environment.GetCommandLineArgs()[0];

            options.InstallDir = RegistryHandler.GetConfig("InstallDir", Path.GetDirectoryName(currentExe)!);
            options.ExePath = Path.Combine(options.InstallDir, Path.GetFileName(currentExe));
            if (options.ExePath.EndsWith(".dll"))
            {
                options.ExePath = Path.ChangeExtension(options.ExePath, "exe");
            }
        }

        public override void Install(InstallOptions options)
        {
            this.GetPaths(options);

            // Add to the PATH
            string? path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(path))
            {
                // Unusual for the PATH to be empty - do nothing.
            }
            else if (!path.Contains(options.InstallDir))
            {
                path = path + Path.PathSeparator + options.InstallDir;
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
            }

            this._registry.ApplyRegistryValues<ExeInstaller>(this.GetRegistryItems(options));
        }

        public override void Remove(UninstallOptions options)
        {
            this.GetPaths(options);

            string? path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            if (path != null && path.Contains(options.InstallDir))
            {
                path = path.Replace(Path.PathSeparator + options.InstallDir, "");
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.User);
            }

            this._registry.RestoreRegistryValues<ExeInstaller>();
            RegistryHandler.DeleteKeyRecursive(@"Software\Classes\HagiGuest.open");
        }
    }
}