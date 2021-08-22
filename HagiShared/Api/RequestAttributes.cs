using System;
using System.Reflection;

namespace HagiShared.Api
{
    /// <summary>
    /// Describes a request
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RequestAttribute : Attribute
    {
        private string? _name;

        public string? NameLower => this.Name?.ToLowerInvariant();

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

        public string? Info { get; set; }

        public RequestAttribute(string path, string? name = null)
        {
            this._name = name;
            this.Path = path.StartsWith('/')
                ? path
                : HostRequest.RootPath + path;
        }
    }

    /// <summary>
    /// Describes a field in a request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OptionAttribute : Attribute
    {
        private bool? _isFlag;
        public string Name { get; }
        public string? Info { get; set; }
        public bool Required { get; set; }
        public bool Hide { get; set; }

        public bool IsFlag
        {
            get => this._isFlag == null ? this.PropertyInfo.PropertyType == typeof(bool) : this._isFlag == true;
            set => this._isFlag = value;
        }

        public PropertyInfo PropertyInfo { get; set; } = null!;

        public bool IsPayload { get; set; }

        public OptionAttribute(string name, string? info = null, bool required = false)
        {
            this.Name = name;
            this.Info = info;
            this.Required = required;
        }
    }
}