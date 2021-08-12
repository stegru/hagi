namespace HostServer.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using GuestIntegration;
    using HagiShared.Extensions;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    public class ShellCommands
    {
        [JsonProperty]
        public ShellCommand GetMountStatus { get; set; } =
            new ShellCommand("./Scripts/share-mount.sh --get", "mount_path");

        [JsonProperty]
        public ShellCommand Mount { get; set; } =
            new ShellCommand("./Scripts/share-mount.sh --mount", "mount_path");

        [JsonProperty]
        public ShellCommand Unmount { get; set; } =
            new ShellCommand("./Scripts/share-mount.sh --unmount", "mount_path");

        [JsonProperty]
        public ShellCommand GetCredentials { get; set; } =
            new ShellCommand("./Scripts/share-mount.sh --auth", "mount_user", "mount_pass");
    }

    [JsonConverter(typeof(ShellCommandConverter))]
    public class ShellCommand
    {
        public const string EnvironmentPrefix = "hagi_";

        public string Command { get; set; }
        private string[] ReturnValues { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public ShellCommand(string command, params string[] returnValues)
        {
            this.Command = command;
            this.ReturnValues = returnValues;
        }

        public async Task<ShellResult> Run(Guest guest, object? environmentData = null)
        {
            return await this.Run(guest.Config, guest, environmentData);
        }

        public async Task<ShellResult> Run(Config config, Guest? guest = null, object? environmentData = null)
        {
            if (string.IsNullOrEmpty(this.Command))
            {
                return new ShellResult() { ExitCode = 0 };
            }

            Dictionary<string, object?> env = JsonUtil.JsonToDict(config.ToJson(), prefix: "config");

            if (guest != null)
            {
                JsonUtil.JsonToDict(guest.ToJson(), env, "guest");
            }

            if (environmentData != null)
            {
                JsonUtil.JsonToDict(environmentData.ToJson(), env);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = Environment.GetEnvironmentVariable("SHELL") ?? "/bin/sh",
                ArgumentList = { "-c", this.Command },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = AppContext.BaseDirectory
            };

            foreach ((string key, object? value) in env)
            {
                string valueText;
                if (value is bool b)
                {
                    valueText = b ? "1" : "0";
                }
                else
                {
                    valueText = value?.ToString() ?? string.Empty;
                }
                startInfo.Environment[$"{ShellCommand.EnvironmentPrefix}{key.Replace('.', '_')}"] = valueText;
            }

            Process process = new Process()
            {
                StartInfo = startInfo
            };

            ShellResult shellResult = new ShellResult();

            StringBuilder stdout = new StringBuilder();

            // Read 'key=value' lines.
            Regex parseLine = new Regex(@"^(?<key>[0-9a-zA-Z_]+)=(?<value>.*)$");
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    stdout.AppendLine(args.Data);
                    Match match = parseLine.Match(args.Data);
                    if (match.Success)
                    {
                        shellResult.Values[match.Groups["key"].Value] = match.Groups["value"].Value;
                    }
                }
            };

            process.Start();

            process.BeginOutputReadLine();

            shellResult.Error = await process.StandardError.ReadToEndAsync();

            try
            {
                await process.WaitForExitAsync().Timeout(this.Timeout);
            }
            catch (TimeoutException)
            {
                process.Kill(true);
            }

            // ReSharper disable once MethodHasAsyncOverload
            process.WaitForExit();

            shellResult.Output = stdout.ToString();
            shellResult.ExitCode = process.ExitCode;
            shellResult.Success = shellResult.ExitCode == 0;

            return shellResult;
        }

    }

    public class ShellResult
    {
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public bool Success { get; set; }

        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string? this[string key]
        {
            get
            {
                return this.Values.TryGetValue(key, out string? value)
                    ? value
                    : null;
            }
        }
    }

    [JsonConverter(typeof(ShellCommand))]
    public class ShellCommandConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ShellCommand command = (ShellCommand)value!;
            writer.WriteValue(command.Command);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            ShellCommand command = existingValue as ShellCommand ?? new ShellCommand(string.Empty);
            command.Command = reader.Value as string ?? string.Empty;
            return command;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ShellCommand);
        }
    }

}