using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Hagi.HostServer.Configuration
{
    public class Paths
    {
        private Dictionary<AppPath, string> _paths = null!;

        public Paths(AppSettings appSettings)
        {
            this.SetDefaultPaths(appSettings);
        }

        public Paths(Dictionary<AppPath, string> paths)
        {
            this._paths = paths;
        }

        private void SetDefaultPaths(AppSettings appSettings)
        {
            this._paths = new Dictionary<AppPath, string>
            {
                [AppPath.Application] = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
                [AppPath.UserConfig] =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        appSettings.UserConfigPath),
            };

            this._paths[AppPath.DefaultConfig] = this.GetPath(AppPath.Application, "DefaultConfig");
        }


        /// <summary>Gets a path special to the application.</summary>
        public string GetPath(AppPath appPath, string? childPath = null)
        {
            return Path.Combine(this._paths[appPath], childPath ?? string.Empty);
        }

        /// <summary>Gets a path to a config file. If it doesn't exist, default content is copied in place.</summary>
        public string GetConfigFile(string filename)
            => this.GetConfigFile(AppPath.UserConfig, filename);

        /// <summary>Gets a path to a config file. If it doesn't exist, default content is copied in place.</summary>
        public string GetConfigFile(AppPath appPath, string filename)
        {
            string path = this.GetPath(appPath, filename);
            if (!File.Exists(path))
            {
                string? directoryName = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                string defaultPath = this.GetDefaultConfigFile(filename);
                File.Copy(defaultPath, path);
            }

            return path;
        }

        public string GetDefaultConfigFile(string filename)
        {
            return this.GetPath(AppPath.DefaultConfig, filename);
        }
    }

    public enum AppPath
    {
        /// <summary>User configuration (writable)</summary>
        UserConfig,
        /// <summary>Default configuration</summary>
        DefaultConfig,
        /// <summary>Directory containing the application executable</summary>
        Application
    }
}