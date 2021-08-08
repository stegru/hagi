using System.Diagnostics;

namespace HagiShared.System
{
    public class Windows : OS
    {
        public override void Open(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = path
            });
        }
    }
}