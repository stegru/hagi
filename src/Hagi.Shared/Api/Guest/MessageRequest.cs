namespace Hagi.Shared.Api.Guest
{
    [Request("message/show", Info = "Displays a message.")]
    public class MessageRequest : HostRequest
    {
        [Option("message", Info = "The message text.")]
        public string Message { get; set; } = null!;

        [Option("title", Info = "The message title.")]
        public string? Title { get; set; }

        [Option("dialog", Info = "Show a dialog, rather than a notification.")]
        public virtual bool Dialog { get; set; }
    }


    [Request("message/ask", Info = "Displays a dialog message, asking a yes or no question.")]
    public class AskRequest : MessageRequest
    {
        [Option("dialog", Hide = true)]
        public override bool Dialog => false;
    }

    public class MessageResponse : HostResponse
    {
    }
    public class AskResponse : MessageResponse
    {
        public AskResponse(bool result)
        {
            this.Result = result;
        }

        public bool Result { get; }
    }
}