using ClientCodeGen.Models;
using ClientCodeGen.TemplateEngine;

namespace ClientCodeGen.Templates
{
    [TemplateFile("Request.razor")]
    public class RequestTemplate : BaseTemplate<RequestModel>
    {
    }
}