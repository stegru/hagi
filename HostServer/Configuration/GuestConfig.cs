namespace HostServer.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using Newtonsoft.Json;

    /// <summary>
    /// Configuration for a guest.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GuestConfig
    {
        [JsonProperty]
        public Dictionary<string, string> PathMappings { get; set; } = null!;

        private List<PathMapping> guestPaths = null!;

        [JsonIgnore]
        internal bool IsDefault => this.GuestId == "*";

        [JsonProperty]
        public string GuestId { get; set; } = null!;
        [JsonProperty]
        public string Secret { get; set; } = null!;


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

        /// <summary>Generates the secret, if there's not one already.</summary>
        internal void GenerateSecret()
        {
            if (string.IsNullOrEmpty(this.Secret))
            {
                using RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] bytes = new byte[15];
                rng.GetBytes(bytes);
                this.Secret = new string(Convert.ToBase64String(bytes).Where(char.IsLetterOrDigit).ToArray());
            }
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