using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using HagiShared.Api;

namespace HostServer.Configuration
{
    /// <summary>
    /// Configuration for a guest.
    /// </summary>
    public class GuestConfig
    {
        public Dictionary<string, string> PathMappings { get; set; } = null!;

        private List<PathMapping> guestPaths = null!;


        [OnDeserialized]
        public void Initialise()
        {
            this.guestPaths = this.PathMappings.Select(kv => new PathMapping(kv.Key, kv.Value))
                .OrderByDescending(mapping => mapping.Guest.Split('/').Length)
                .ToList();
        }

        public static string NormalisePath(string path, bool? trailingSlash = false)
        {
            string p = path.Replace('\\', '/');

            return trailingSlash switch
            {
                false => p.TrimEnd('/'),
                true when !p.EndsWith("/") => p + "/",
                _ => p
            };
        }

        public string? MapPath(string guestPath)
        {
            string path = GuestConfig.NormalisePath(guestPath, true);

            PathMapping? mapping = this.guestPaths.FirstOrDefault(m => path.StartsWith(m.Guest));

            if (mapping != null)
            {
                string subPath = path[(mapping.Guest.Length - 0)..];
                string fullPath = Path.Join(mapping.Host, subPath);
                return fullPath;
            }

            return null;
        }
    }

    public class PathMapping
    {
        public PathMapping(string host, string guest)
        {
            this.Host = GuestConfig.NormalisePath(host);
            this.Guest = GuestConfig.NormalisePath(guest, true);
        }

        public string Host { get; }
        public string Guest { get; }
    }

}