using System;

namespace ClientCodeGen.TemplateEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class TemplateFileAttribute : Attribute
    {
        public string Filename { get; }

        public TemplateFileAttribute(string filename)
        {
            this.Filename = filename;
        }
    }
}