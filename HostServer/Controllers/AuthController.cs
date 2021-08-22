using HostServer.Configuration;

namespace HostServer.Controllers
{
    using HagiShared.Api;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("hagi/auth")]
    public class AuthController : HagiControllerBase
    {
        public AuthController(ILogger<OpenController> logger, Config config) : base(logger, config)
        {
        }

        [HttpPost("join")]
        public FileMapResponse Join(FileMapRequest request)
        {
            GuestConfig config = this.GuestConfig(request);

            string? mapping = config.MapPath(request.Path);
            return new FileMapResponse(mapping);
        }
    }
}