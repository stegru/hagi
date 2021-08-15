using System.Diagnostics;

namespace HagiShared.System
{
    public class Linux : OS
    {
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