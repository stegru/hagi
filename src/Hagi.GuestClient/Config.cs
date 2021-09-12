namespace Hagi.HagiGuest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Simple config file for "key=value" items.
    /// </summary>
    public class Config
    {
        private static readonly string ConfigFile = Config.GetConfigFilePath("guest.conf");

        /// <summary>Gets the path to file in the config directory.</summary>
        public static string GetConfigFilePath(string? filename = null) =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "hagi-guest",
                filename ?? string.Empty);

        private readonly string _file;

        public static Config Current { get; } = new Config(Config.ConfigFile);

        private readonly Dictionary<string, string?> values = new Dictionary<string, string?>();
        private readonly HashSet<string> updated = new HashSet<string>();

        private Config(string file)
        {
            this._file = file;
            this.LoadFile();
        }

        /// <summary>
        /// The value, or null if not found.
        /// </summary>
        public string? this[string key]
        {
            get => this.values.TryGetValue(key, out string? value)
                ? value
                : null;
            set
            {
                this.updated.Add(key);
                this.values[key] = value;
            }
        }

        private void LoadFile()
        {
            if (File.Exists(this._file))
            {
                foreach (string line in File.ReadAllLines(this._file))
                {
                    (string key, string value) = this.ParseLine(line);
                    this.values[key] = value;
                }
            }

            this.updated.Clear();
        }

        public void SaveFile()
        {
            if (this.updated.Count > 0)
            {
                List<string> newLines = new List<string>();

                if (File.Exists(this._file))
                {
                    // Read each line, only changing the lines containing the values that have been updated.
                    foreach (string line in File.ReadAllLines(this._file))
                    {
                        (string key, string value) = this.ParseLine(line);

                        string? newLine;
                        if (!string.IsNullOrEmpty(key) && this.updated.Contains(key))
                        {
                            this.updated.Remove(key);

                            string? newValue = this[key];
                            newLine = newValue == null
                                ? null
                                : $"{key}={newValue}";
                        }
                        else
                        {
                            newLine = line;
                        }

                        if (newLine != null)
                        {
                            newLines.Add(newLine);
                        }
                    }
                }

                // Write the new values
                foreach (string key in this.updated)
                {
                    string? newValue = this[key];
                    if (newValue != null)
                    {
                        newLines.Add($"{key}={newValue}");
                    }
                }

                string? directoryName = Path.GetDirectoryName(this._file);
                if (directoryName != null)
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllLines(this._file, newLines);
            }
        }

        private readonly Regex _parseLine = new Regex(@"^\s*(?<key>[^;#=][^=]*)\s*=\s*(?<value>.*)\s*$");

        /// <summary>
        /// Parse a "key=value" line.
        /// </summary>
        private (string key, string value) ParseLine(string line)
        {
            Match match = this._parseLine.Match(line);
            return match.Success
                ? (key: match.Groups["key"].Value, value: match.Groups["value"].Value)
                : (key: string.Empty, value: string.Empty);
        }
    }
}