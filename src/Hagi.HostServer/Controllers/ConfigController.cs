namespace Hagi.HostServer.Controllers
{
    using System.Net;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using Auth;
    using Configuration;
    using GuestIntegration;
    using Shared.Api;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Shared.Api.Config;
    using UI;

    [ApiController]
    [Route("hagi/config")]
    [ConfigAuth]
    public class ConfigController : HagiControllerBase
    {
        public ConfigController(ILogger<OpenController> logger, Config config) : base(logger, config)
        {
        }

        [HttpGet("start")]
        [NoAuth]
        public StartConfigResponse Start()
        {
            return new StartConfigResponse(this.Config.ConfigPasswordHash != null, this.Config.InitialSetupComplete);
        }

        [HttpPost("login")]
        [NoAuth]
        public LoginResponse Login(LoginRequest request)
        {
            ConfigAuthMiddleware.Authenticate(this.Config, request.Username, request.Password);
            return new LoginResponse(ConfigAuthMiddleware.NewToken());
        }

        [HttpPost("password")]
        [NoAuth]
        public ConfigResponse SetPassword(PasswordRequest request)
        {
            if (!this.Config.InitialSetupComplete && this.Config.ConfigPasswordHash != null)
            {
                ConfigAuthMiddleware.Authenticate(this.Config, request.Username!, request.Password!);
            }

            this.Config.SetPassword(request.NewUsername, request.NewPassword);

            return new ConfigResponse();
        }

        [HttpGet]
        public GetConfigResponse GetConfig()
        {
            return new GetConfigResponse()
            {
                Config = this.Config.ToJson()
            };
        }
    }
}