using HostServer.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HostServer.Controllers
{
    using Auth;
    using GuestIntegration;

    public class HagiControllerBase : ControllerBase
    {
        protected readonly ILogger<OpenController> Logger;
        protected readonly Config Config;

        protected Guest Guest => this.HttpContext.GetGuest();

        public HagiControllerBase(ILogger<OpenController> logger, Config config)
        {
            this.Logger = logger;
            this.Config = config;
        }

    }
}