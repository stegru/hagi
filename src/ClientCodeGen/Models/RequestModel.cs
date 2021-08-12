using System;
using System.Collections.Generic;
using Hagi.Shared.Api;

namespace ClientCodeGen.Models
{
    public class RequestModel
    {
        public RequestModel(RequestAttribute request, Type requestType)
        {
            this.Request = request;
            this.RequestType = requestType;
        }

        public RequestAttribute Request { get; set; }
        public List<OptionAttribute> Options { get; set; } = new List<OptionAttribute>();

        public Type RequestType { get; set; }
    }
}