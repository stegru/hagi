using HagiShared.Api;
using HostServer.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HostServer.Controllers
{
    public class HagiControllerBase : ControllerBase
    {
        protected readonly ILogger<OpenController> Logger;
        protected readonly Config Config;

        public HagiControllerBase(ILogger<OpenController> logger, Config config)
        {
            this.Logger = logger;
            this.Config = config;
        }

        protected GuestConfig GuestConfig(HostRequest request)
        {
            return this.Config.GetGuest(request.Guest);
        }

    }
}