using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HagiShared.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace HostServer.Configuration
{
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
        public Dictionary<string, GuestConfig> GuestConfig { get; set; } = new Dictionary<string, GuestConfig>();

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


            this.Load(configFile);
        }

        public void Load(string configFile)
        {
            this.Logger.LogInformation("Loading config {file}", new { file = configFile });
            using TextReader reader = File.OpenText(configFile);
            JsonSerializer.Create().Populate(new JsonTextReader(reader), this);
        }

        public GuestConfig GetGuest(string guestId)
        {
            return this.GuestConfig.Values.First();
        }
    }


}