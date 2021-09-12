using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hagi.HostServer.Controllers
{
    using Auth;
    using Configuration;
    using GuestIntegration;

    public abstract class HagiControllerBase : ControllerBase
    {
        protected readonly ILogger<OpenController> Logger;
        protected readonly Config Config;

        public HagiControllerBase(ILogger<OpenController> logger, Config config)
        {
            this.Logger = logger;
            this.Config = config;
        }
    }

    public abstract class GuestController : HagiControllerBase
    {
        protected Guest Guest => this.HttpContext.GetGuest();

        protected GuestController(ILogger<OpenController> logger, Config config) : base(logger, config)
        {
        }
    }
}