using System;
using System.Collections.Generic;
using HagiShared.Api;

namespace ClientCodeGen.Models
{
    public class RequestModel
    {
        public RequestAttribute Request { get; set; }
        public List<OptionAttribute> Options { get; set; } = new List<OptionAttribute>();

        public Type RequestType { get; set; }
    }
}