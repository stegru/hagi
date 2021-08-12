namespace Hagi.Shared.Platform
{
    using System;

    // ReSharper disable once InconsistentNaming
    public abstract class OS
    {
        public static OS Current { get; }

        static OS()
        {
            if (OperatingSystem.IsWindows())
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

        public abstract Platform Platform { get; }
        public abstract string UserId { get; }
        public abstract void Open(string path);
    }

    public enum Platform
    {
        Linux,
        Windows
    }
}