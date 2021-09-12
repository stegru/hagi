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
    using Shared.Api.Guest;
    using UI;

    [ApiController]
    [Route("hagi/auth")]
    public class AuthController : GuestController
    {
        public AuthController(ILogger<OpenController> logger, Config config) : base(logger, config)
        {
        }

        [HttpPost("join")]
        [NoAuth]
        public async Task<JoinResponse> Join(JoinRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Guest) || request.Guest == "*")
            {
                throw new ApiException("Invalid guest name", HttpStatusCode.BadRequest);
            }

            if (request.SharedSecret != null)
            {
                if (request.SharedSecret != this.Config.SharedSecret)
                {
                    throw new AuthenticationException("Invalid secret");
                }
            }
            else if (this.Guest == null!)
            {
                UserMessage userMessage =
                    new UserMessage(
                        $"Guest machine wants to access the host.\n\nName: <tt>{request.MachineName}</tt>\nID: <tt>{request.Guest}</tt>\n\n<big>Grant access?</big>")
                    {
                        Question = true
                    };

                bool allow = await userMessage.Show();
                if (!allow)
                {
                    throw new AuthenticationException("Access rejected by user");
                }
            }

            Guest guest = this.Config.GetGuest(request.Guest, true) ?? this.Config.AddGuest(request.Guest);
            guest.GenerateSecret();
            guest.MachineName = request.MachineName;

            await guest.MountShare(true);

            this.Config.Save();

            return new JoinResponse(guest.GuestId, guest.Secret);
        }
    }
}