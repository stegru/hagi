namespace Hagi.HostServer.Controllers
{
    using System.Threading.Tasks;
    using Configuration;
    using Shared.Api;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Shared.Api.Guest;
    using UI;

    /// <summary>
    /// Displays messages to the user.
    /// </summary>
    [ApiController]
    [Route("hagi/message")]
    public class MessageController : GuestController
    {
        public MessageController(ILogger<OpenController> logger, Config config) : base(logger, config)
        {
        }

        /// <summary>
        /// Show a message.
        /// </summary>
        [HttpPost("show")]
        public MessageResponse ShowMessage(MessageRequest request)
        {
            UserMessage message = MessageController.MakeMessage(request);

            _ = message.Show();

            return new MessageResponse();
        }

        /// <summary>
        /// Show a message dialog, with yes/no buttons and respond with the reply.
        /// </summary>
        [HttpPost("ask")]
        public async Task<MessageResponse> ShowQuestion(AskRequest request)
        {
            UserMessage message = MessageController.MakeMessage(request);

            bool answer = await message.Show();

            return new AskResponse(answer);
        }

        /// <summary>Create a message from a request.</summary>
        private static UserMessage MakeMessage(MessageRequest request)
        {
            return new UserMessage(request.Message)
            {
                Title = request.Title,
                Dialog = request.Dialog,
                Question = request is AskRequest,
            };
        }
    }
}