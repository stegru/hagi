namespace HostServer.Controllers
{
    using System.Net;
    using System.Security.Authentication;
    using Auth;
    using Configuration;
    using HagiShared.Api;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    public class ErrorController : HagiControllerBase
    {
        public ErrorController(ILogger<OpenController> logger, Config config) : base(logger, config)
        {
        }

        [NoAuth]
        [Route("/error")]
        public ErrorResponse ErrorHandler()
        {
            IExceptionHandlerFeature ex = this.HttpContext.Features.Get<IExceptionHandlerFeature>();

            this.Response.StatusCode = ex.Error switch
            {
                ApiException apiException => (int)apiException.StatusCode,
                AuthenticationException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            return new ErrorResponse(ex.Error)
            {
                StatusCode = this.Response.StatusCode
            };
        }
    }
}