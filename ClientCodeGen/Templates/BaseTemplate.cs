using ClientCodeGen.TemplateEngine;

namespace ClientCodeGen.Templates
{
    public class BaseTemplate<T> : Generator<T>
    {
    }

    public class BaseTemplate : BaseTemplate<object>
    {
        
    }
}