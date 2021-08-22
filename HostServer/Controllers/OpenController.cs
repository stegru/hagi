using HostServer.Configuration;

namespace HostServer.Controllers
{
    using System;
    using HagiShared.Api;
    using HagiShared.System;
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
            GuestConfig config = this.GuestConfig(request);

            string? mapping = config.MapPath(request.Path);
            return new FileMapResponse(mapping);
        }

        [HttpPost("open")]
        public HostResponse Post(OpenRequest openRequest)
        {
            GuestConfig config = this.GuestConfig(openRequest);

            string? path = openRequest.Path;

            if (openRequest.Type == OpenPathType.Any)
            {
                if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uri))
                {
                    openRequest.Type = uri.IsFile ? OpenPathType.File : OpenPathType.Url;
                }
            }

            if (openRequest.Type == OpenPathType.File)
            {
                path = config.MapPath(path);
            }

            OpenResponse response = new OpenResponse();

            if (path != null)
            {
                OS.Current.Open(path);
            }
            else
            {
                response.Failed = true;
            }

            return response;
        }
    }
}