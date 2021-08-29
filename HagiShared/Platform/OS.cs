using System.Runtime.InteropServices;

namespace HagiShared.Platform
{
    using System;

    // ReSharper disable once InconsistentNaming
    public abstract class OS
    {
        public static OS Current { get; }

        static OS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                OS.Current = new Windows();
            }
            else
            {
                OS.Current = new Linux();
            }
        }

        protected OS()
        {
            this.Name = this.GetType().Name;
        }

        public string Name { get; }

        public abstract void Open(string path);
    }
}