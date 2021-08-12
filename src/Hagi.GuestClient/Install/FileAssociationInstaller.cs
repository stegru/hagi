namespace Hagi.HagiGuest.Install
{
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using Shared.Platform;

    /// <summary>
    /// Add a shell command to all files.
    /// </summary>
    [Installer(Platform.Windows)]
    [SupportedOSPlatform("windows")]
    public class FileAssociationInstaller : Installer
    {
        private readonly RegistryHandler _registry = new RegistryHandler();

        private const string ShellKey = @"SOFTWARE\Classes\*\shell\hagi-guest";

        private static IEnumerable<RegistryHandler.RegistryItem> GetRegistryItems(InstallOptions options)
        {
            return new[]
            {
                new RegistryHandler.RegistryItem(FileAssociationInstaller.ShellKey, "", "Open in host"),
                new RegistryHandler.RegistryItem($@"{FileAssociationInstaller.ShellKey}\command", "",
                    $"\"{options.ExePath}\" open --file \"%1\"")
            };
        }

        public override void Install(InstallOptions options)
        {
            this._registry.ApplyRegistryValues<FileAssociationInstaller>(
                FileAssociationInstaller.GetRegistryItems(options));
        }

        public override void Remove(UninstallOptions options)
        {
            this._registry.RestoreRegistryValues<FileAssociationInstaller>();
            RegistryHandler.DeleteKeyRecursive(FileAssociationInstaller.ShellKey);
        }
    }
}