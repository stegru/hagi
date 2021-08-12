using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HostServer.Configuration
{
    using System;
    using GuestIntegration;

    /// <summary>
    /// Host server configuration.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Config
    {
        public ILogger<Config> Logger { get; }
        public Paths Paths { get; }
        public AppSettings AppSettings { get; }

        public int Port { get; private set; } = 5580;

        [JsonProperty]
        public Dictionary<string, Guest> Guests { get; set; } = new Dictionary<string, Guest>();

        [JsonProperty]
        public string? SharedSecret { get; set; }

        [JsonProperty]
        public ShellCommands Commands { get; set; } = new ShellCommands();

        public Config(ILogger<Config> logger, Paths paths, AppSettings appSettings, IConfiguration configuration)
        {
            this.Logger = logger;
            this.Paths = paths;
            this.AppSettings = appSettings;

            string configFile = this.Paths.GetConfigFile("hagi-host.json5");

            string value = configuration.GetValue<string>("Urls");

            if (!string.IsNullOrEmpty(value))
            {
                Regex portRegex = new Regex(@":([0-9]+)");
                if (int.TryParse(portRegex.Match(value).Groups[1].Value, out int port))
                {
                    this.Port = port;
                }
            }


            //this.Load(configFile);
            this.Load(this.Paths.GetDefaultConfigFile("hagi-host.json5"));
            this.Save(configFile);
        }

        public void Load(string configFile)
        {
            this.Logger.LogInformation("Loading config {file}", new { file = configFile });
            using TextReader reader = File.OpenText(configFile);
            JsonSerializer.Create().Populate(new JsonTextReader(reader), this);

            if (!this.Guests.ContainsKey("*"))
            {
                this.Guests["*"] = new Guest();
            }

            foreach ((string key, Guest guest) in this.Guests)
            {
                guest.Config = this;
                guest.GuestId = key;
            }
        }

        public void Save(string configFile)
        {
            using TextWriter writer = new StreamWriter(configFile);
            JsonSerializer.Create().Serialize(writer, this);
            writer.Close();
        }

        /// <summary>
        /// Gets configuration for a guest.
        /// </summary>
        public Guest GetGuest(string guestId = "*")
        {
            return this.Guests.TryGetValue(guestId, out Guest? guest)
                ? guest
                : this.Guests["*"];
        }

        /// <summary>
        /// Gets configuration for a guest.
        /// </summary>
        public Guest? GetGuest(string guestId, bool noFallback)
        {
            return this.Guests.TryGetValue(guestId, out Guest? guest)
                ? guest
                : noFallback
                    ? null
                    : this.Guests["*"];
        }

        /// <summary>
        /// Adds a new guest.
        /// </summary>
        public Guest AddGuest(string guestId)
        {
            if (this.Guests.ContainsKey(guestId))
            {
                throw new ApplicationException($"Guest '{guestId}' already exists.");
            }

            Guest guest = new Guest()
            {
                GuestId = guestId,
                Config = this
            };

            this.Guests.Add(guestId, guest);
            return guest;
        }
    }


}