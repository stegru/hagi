namespace Hagi.HostServer.UI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using GuestIntegration;

    /// <summary>
    /// Displays a message to the user.
    /// </summary>
    public class UserMessage
    {
        public string Message { get; set; }
        public string? Title { get; init; }
        public bool Dialog { get; init; }
        public bool Question { get; init; }

        public DialogType DialogType { get; set; } = DialogType.Info;

        public UserMessage(string message)
        {
            this.Message = message;
        }

        public async Task<bool> Show()
        {
            List<string> args = this.BuildCommand();
            string file = args.First();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = args.First()
            };

            foreach (string arg in args.Skip(1))
            {
                startInfo.ArgumentList.Add(arg);
            }

            using Process process = new Process()
            {
                StartInfo = startInfo
            };

            int exitCode = await process.StartAsync();

            return exitCode == 0 || (this.Question && exitCode == 1)
                ? true
                : throw new ApplicationException($"{file} returned exit code {exitCode}");
        }

        private List<string> BuildCommand()
        {
            List<string> args = new List<string>
            {
                "zenity",
                $"--text={this.Message}",
                $"--title={this.Title}",
                $"--width=200"
            };

            if (this.Question)
            {
                args.Add("--question");
            }
            else if (this.Dialog)
            {
                string type = this.DialogType switch
                {
                    DialogType.Error => "--error",
                    DialogType.Warning => "--warning",
                    DialogType.Info => "--info",
                    _ => throw new ArgumentOutOfRangeException()
                };

                args.Add(type);
            }
            else
            {
                args.Add("--notification");
            }

            return args;
        }
    }

    public enum DialogType
    {
        Error,
        Info,
        Warning
    }
}