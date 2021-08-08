namespace HostServer.Controllers
{
    using System;
    using HagiShared.Api;
    using HagiShared.System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("hagi")]
    public class OpenController : ControllerBase
    {
        private readonly ILogger<OpenController> _logger;
        private readonly Config _config;

        public OpenController(ILogger<OpenController> logger, Config config)
        {
            this._logger = logger;
            this._config = config;
        }

        [HttpPost("map")]
        public FileMapResponse Map(FileMapRequest request)
        {
            string? mapping = this._config.MapPath(request.Path);
            return new FileMapResponse(mapping);
        }

        [HttpPost("open")]
        public HostResponse Post(OpenRequest openRequest)
        {
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
                path = this._config.MapPath(path);
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