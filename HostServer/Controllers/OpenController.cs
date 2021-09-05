using HostServer.Configuration;

namespace HostServer.Controllers
{
    using System;
    using HagiShared.Api;
    using HagiShared.Platform;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Opens something on this machine.
    /// </summary>
    [ApiController]
    [Route("hagi")]
    public class OpenController : HagiControllerBase
    {
        public OpenController(ILogger<OpenController> logger, Config config) : base(logger, config)
        {
        }

        [HttpPost("map")]
        public FileMapResponse Map(FileMapRequest request)
        {
            GuestConfig config = this.GuestConfig;

            string? mapping = config.MapPath(request.Path);
            return new FileMapResponse(mapping);
        }

        [HttpPost("open")]
        public HostResponse Post(OpenRequest openRequest)
        {
            GuestConfig config = this.GuestConfig;

            string path;

            if (openRequest.File)
            {
                path = config.MapPath(openRequest.Path)
                    ?? throw new ApiException($"Unable to map '{openRequest.Path}'");
            }
            else
            {
                path = openRequest.Path;
                if (openRequest.Url && !Uri.TryCreate(path, UriKind.Absolute, out Uri? uri))
                {
                    throw new ApiException($"{nameof(openRequest.Url)} does not look like a url");
                }
            }

            OpenResponse response = new OpenResponse();

            OS.Current.Open(path);

            return response;
        }
    }
}