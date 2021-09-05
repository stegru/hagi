using System.Diagnostics;
using NotImplementedException = System.NotImplementedException;

namespace HagiShared.System
{
    using global::System.Runtime.InteropServices;
    using Platform;

    public class Windows : OS
    {
        public override Platform Platform => Platform.Windows;

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