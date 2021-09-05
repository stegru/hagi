using System.Diagnostics;

namespace HagiShared.System
{
    using global::System.Runtime.InteropServices;
    using Platform;

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
    }
}