using System;
using System.Reflection;

namespace HagiShared.Api
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RequestAttribute : Attribute
    {
        private string? _name;

        public string? Name
        {
            get
            {
                if (this.RequestType != null && this._name == null)
                {
                    return this.RequestType.Name.EndsWith("Request")
                        ? this.RequestType.Name[..^"Request".Length]
                        : this.RequestType.Name;
                }

                return this._name;
            }
            set => this._name = value;
        }

        public string Path { get; }

        public Type? RequestType { get; set; }

        public RequestAttribute(string path, string? name = null)
        {
            this._name = name;
            this.Path = path.StartsWith('/')
                ? path
                : HostRequest.RootPath + path;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OptionAttribute : Attribute
    {
        public string Name { get; }
        public bool Required { get; set; }

        public PropertyInfo PropertyInfo { get; set; } = null!;

        public bool IsPayload { get; set; }

        public OptionAttribute(string name, bool required = false)
        {
            this.Name = name;
            this.Required = required;
        }
    }
}