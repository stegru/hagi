namespace HagiShared.Platform
{
    using global::System;
    using global::System.Diagnostics;

    public class Linux : OS
    {
        public override Platform Platform => Platform.Linux;

        public override void Open(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "xdg-open",
                Arguments = path
            });
        }

        public override string UserId => Environment.GetEnvironmentVariable("UID")
                                         ?? throw new ApplicationException("$UID is not defined");
    }
}