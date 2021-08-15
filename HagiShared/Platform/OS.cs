using System.Runtime.InteropServices;

namespace HagiShared.System
{
    public abstract class OS
    {
        public static OS Current { get; }

        static OS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                OS.Current = new Linux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS.Current = new Windows();
            }
        }


        public abstract void Open(string path);
    }
}