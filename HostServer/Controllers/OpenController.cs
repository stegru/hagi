using HostServer.Configuration;

namespace HostServer.Controllers
{
    using System;
    using System.Threading.Tasks;
    using GuestIntegration;
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
            string? mapping = this.Guest.MapPath(request.Path);
            return new FileMapResponse(mapping);
        }

        [HttpPost("open")]
        public async Task<HostResponse> Post(OpenRequest openRequest)
        {
            string path;

            if (openRequest.File)
            {
                bool mounted = await this.Guest.EnsureMounted();
                path = this.Guest.MapPath(openRequest.Path)
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