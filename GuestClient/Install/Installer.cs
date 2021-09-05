namespace Hagi.HagiGuest.Install
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using HagiShared.Platform;

    public abstract class Installer
    {
        public abstract void Install(InstallOptions options);
        public abstract void Remove(UninstallOptions options);

        private static IEnumerable<Type> GetInstallerTypes()
        {
            return new[]
            {
                typeof(ExeInstaller),
                typeof(FileAssociationInstaller),
                typeof(BrowserInstaller),
                typeof(ShareInstaller)
            };
        }
        
        private static IEnumerable<Installer> GetAllInstallers()
        {
            IEnumerable<Type> installerTypes = Installer.GetInstallerTypes();

            foreach (Type installerType in installerTypes)
            {
                InstallerAttribute? installerAttribute = installerType.GetCustomAttribute<InstallerAttribute>();
                if (installerAttribute?.Platform != OS.Current.Platform)
                {
                    continue;
                }

                Console.WriteLine($"Starting {installerType.Name}");
                Installer installer = Activator.CreateInstance(installerType) as Installer
                                      ?? throw new ApplicationException(
                                          $"Unable to create instance of {installerType.Name}");
                yield return installer;
            }
        }

        public static void InstallAll(InstallOptions installOptions)
        {
            foreach (Installer installer in Installer.GetAllInstallers())
            {
                installer.Install(installOptions);
            }
        }
        public static void RemoveAll(UninstallOptions uninstallOptions)
        {
            foreach (Installer installer in Installer.GetAllInstallers())
            {
                installer.Remove(uninstallOptions);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    sealed class InstallerAttribute : Attribute
    {
        public Platform Platform { get; }

        public InstallerAttribute(Platform platform)
        {
            this.Platform = platform;
        }
    }
}