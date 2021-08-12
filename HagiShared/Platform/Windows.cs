using NotImplementedException = System.NotImplementedException;

namespace HagiShared.Platform
{
    using System.Diagnostics;

    public class Windows : OS
    {
        public override Platform Platform => Platform.Windows;

        public override string UserId => throw new NotImplementedException();

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