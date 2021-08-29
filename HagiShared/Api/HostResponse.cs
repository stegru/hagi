using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace HagiShared.Api
{
    /// <summary>A response from the host.</summary>
    public class HostResponse
    {
        public virtual bool Success => true;

        public HostResponse()
        {
        }
    }

    /// <summary>An error response from the host.</summary>
    public class ErrorResponse : HostResponse
    {
        [JsonIgnore]
        public Exception? Exception { get; }
        public string? Message { get; set; }

        public int StatusCode { get; set; }

        public override bool Success => false;

        public ErrorResponse() : this(null)
        {
        }

        public ErrorResponse(Exception? exception, HttpStatusCode statusCode = 0, string? message = null)
        {
            this.Exception = exception;
            this.Message = message ?? exception?.Message;
        }
    }

    /// <summary>Thrown when there's problem with a request.</summary>
    [Serializable]
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

        public ApiException()
        {
        }

        public ApiException(string message, HttpStatusCode statusCode) : this(message)
        {
            this.StatusCode = statusCode;
        }
        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ApiException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}