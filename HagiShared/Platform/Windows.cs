using System.Diagnostics;

namespace HagiShared.System
{
    using Platform;

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