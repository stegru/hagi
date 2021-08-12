using ClientCodeGen.TemplateEngine;

namespace ClientCodeGen.Templates
{
    using System.IO;
    using System.Runtime.CompilerServices;

    public class BaseTemplate<T> : Generator<T>
    {
        protected string RelativePath(string path, [CallerFilePath]string? callerFilePath = null)
        {
            return Path.Combine(Path.GetDirectoryName(callerFilePath)!, path);
        }
    }

    public class BaseTemplate : BaseTemplate<object>
    {
        
    }
}