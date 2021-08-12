namespace Hagi.HostServer.GuestIntegration
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Configuration;
    using Shared.Extensions;
    using Newtonsoft.Json;

    /// <summary>Mounts network shares provided by guest machines</summary>
    public static class ShareMount
    {
        [JsonObject]
        public class ShareCredentials
        {
            [JsonProperty]
            public string? User { get; set; }

            /// <summary>
            /// The guest password is insecurely stored - it's assumed anyone with access to the host will have access to the guest.
            /// </summary>
            [JsonProperty("Password")]
            public string? StoredPassword
            {
                get => this.Password.ToBase64();
                set => this.Password = value.FromBase64();
            }

            [JsonIgnore]
            public string? Password { get; set; }
        }

        public const string DefaultShareName = "hagi";

        /// <summary>Gets the directory where the machine's share is mounted.</summary>
        private static async Task<(string mountPath, bool mounted)> GetMountStatus(this Guest guest)
        {
            ShellResult result = await guest.Config.Commands.GetMountStatus.Run(guest);

            return (result["mount_path"] ?? string.Empty, result["mounted"].ToBool());
        }

        /// <summary>
        /// Checks if the share is mounted.
        /// </summary>
        public static async Task<bool?> CheckMounted(this Guest guest)
        {
            bool? mounted;
            try
            {
                if (string.IsNullOrEmpty(guest.MountPath))
                {
                    mounted = false;
                }
                else
                {
                    Task<bool> t = Task.Run(() => Directory.Exists(guest.MountPath));
                    mounted = await t.Timeout(TimeSpan.FromSeconds(3));
                }
            }
            catch (TimeoutException)
            {
                // Timeout means it's mounted, but the guest machine is down.
                mounted = null;
            }

            return mounted;
        }

        /// <summary>Mounts the guest share, if it's not already mounted.</summary>
        /// <returns>false if it failed.</returns>
        public static async Task<bool> EnsureMounted(this Guest guest)
        {
            bool? mountState = await guest.CheckMounted();

            return mountState == false
                ? await guest.MountShare(true)
                : mountState == true;
        }

        /// <summary>Mount or unmount the guest share.</summary>
        public static async Task<bool> MountShare(this Guest guest, bool mount)
        {
            if (mount && (guest.ShareCredentials.User == null || guest.ShareCredentials.Password == null))
            {
                ShellResult credentials = await guest.Config.Commands.GetCredentials.Run(guest);
                if (credentials.Success)
                {
                    guest.ShareCredentials.User = credentials["mount_user"];
                    guest.ShareCredentials.Password = credentials["mount_pass"];
                }
            }


            ShellResult result = mount
                ? await guest.Config.Commands.Mount.Run(guest, new
                {
                    ShareUser = guest.ShareCredentials.User,
                    SharePass = guest.ShareCredentials.Password
                })
                : await guest.Config.Commands.Unmount.Run(guest);

            if (mount)
            {
                guest.MountPath = result["mount_path"];
            }
            else
            {
                guest.MountPath = null;
            }

            return result.Success;
        }
    }
}