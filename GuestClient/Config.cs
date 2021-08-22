using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GuestClient
{
    public class Config
    {
        private static readonly string DefaultFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "hagi-guest", "guest.ini");

        private readonly string _file;

        public static Config Current { get; set; }

        public static void Load(string? file = null)
        {
            Config.Current = new Config(file ?? Config.DefaultFile);
        }

        private Dictionary<string, string?> values = new Dictionary<string, string?>();
        private HashSet<string> updated = new HashSet<string>();

        private Config(string file)
        {
            this._file = file;
            this.LoadFile();
        }

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

        private void SaveFile()
        {
            if (this.updated.Count > 0)
            {
                List<string> newLines = new List<string>();

                foreach (string line in File.ReadAllLines(this._file))
                {
                    (string key, string value) = this.ParseLine(line);

                    string? newLine;
                    if (!string.IsNullOrEmpty(key) && this.updated.Contains(key))
                    {
                        this.updated.Remove(key);
                        string? newValue = this[key];
                        if (newValue == null)
                        {
                            newLine = null;
                        }
                        else
                        {
                            newLine = $"{key}={newValue}";
                        }
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

                foreach (string key in this.updated)
                {
                    string? newValue = this[key];
                    if (newValue != null)
                    {
                        newLines.Add($"{key}={newValue}");
                    }
                }


                Directory.CreateDirectory(Path.GetDirectoryName(this._file));
                File.WriteAllLines(this._file, newLines);
            }
        }

        private readonly Regex _parseLine = new Regex(@"^\s*(?<key>[^;#=][^=]*)\s*=\s*(?<value>.*)\s*$");

        private (string key, string value) ParseLine(string line)
        {
            Match match = this._parseLine.Match(line);
            return match.Success
                ? (key: match.Groups["key"].Value, value: match.Groups["value"].Value)
                : (key: string.Empty, value: string.Empty);
        }
    }
}