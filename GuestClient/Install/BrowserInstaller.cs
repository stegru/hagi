namespace Hagi.HagiGuest.Install
{
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using HagiShared.Platform;

    /// <summary>
    /// Installs this executable as a browser.
    /// </summary>
    [Installer(Platform.Windows)]
    [SupportedOSPlatform("windows")]
    public class BrowserInstaller : Installer
    {
        private readonly RegistryHandler _registry = new RegistryHandler();

        private static IEnumerable<RegistryHandler.RegistryItem> GetRegistryItems(InstallOptions options)
        {
            string appId = "HagiGuest.open";
            string client = @$"SOFTWARE\Clients\StartMenuInternet\hagi";
            string capabilities = @$"{client}\Capabilities";
            return new[]
            {
                new RegistryHandler.RegistryItem(client, "", "Host Browser"),
                new RegistryHandler.RegistryItem(@"SOFTWARE\RegisteredApplications", "Host Browser", capabilities),
                new RegistryHandler.RegistryItem(capabilities, "ApplicationDescription", "Open on the host"),
                new RegistryHandler.RegistryItem(capabilities, "ApplicationName", "Host Browser"),
                new RegistryHandler.RegistryItem($@"{capabilities}\FileAssociations", ".htm", appId),
                new RegistryHandler.RegistryItem($@"{capabilities}\FileAssociations", ".html", appId),
                new RegistryHandler.RegistryItem($@"{capabilities}\URLAssociations", "http", appId),
                new RegistryHandler.RegistryItem($@"{capabilities}\URLAssociations", "https", appId),
                new RegistryHandler.RegistryItem($@"{capabilities}\URLAssociations", "mailto", appId),
                new RegistryHandler.RegistryItem($@"{capabilities}\URLAssociations", "ftp", appId),
            };
        }

        public override void Install(InstallOptions options)
        {
            this._registry.ApplyRegistryValues<BrowserInstaller>(BrowserInstaller.GetRegistryItems(options));
        }

        public override void Remove(UninstallOptions options)
        {
            this._registry.RestoreRegistryValues<BrowserInstaller>();
        }
    }
}