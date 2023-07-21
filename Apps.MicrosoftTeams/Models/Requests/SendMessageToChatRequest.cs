using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class SendMessageToChatRequest
    {
        [Display("Chat ID")]
        public string ChatId { get; set; }

        public string Message { get; set; }
    }
}
