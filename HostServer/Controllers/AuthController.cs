﻿using HostServer.Configuration;

namespace HostServer.Controllers
{
    using System.Net;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using Auth;
    using HagiShared.Api;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using UI;

    [ApiController]
    [Route("hagi/auth")]
    public class AuthController : HagiControllerBase
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
            else
            {
                UserMessage userMessage = new UserMessage($"Guest machine wants to access the host.\n\nID: <tt>{request.Guest}</tt>\n\n<big>Grant access?</big>")
                {
                    Question = true
                };

                bool allow = await userMessage.Show();
                if (!allow)
                {
                    throw new AuthenticationException("Access rejected by user");
                }
            }

            GuestConfig guest = this.Config.GetGuest(request.Guest, true) ?? this.Config.AddGuest(request.Guest);
            guest.GenerateSecret();

            return new JoinResponse(guest.GuestId, guest.Secret);
        }
    }
}