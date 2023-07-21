using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetChatMessageRequest
    {
        [Display("Chat ID")]
        public string ChatId { get; set; }
        
        [Display("Message ID")]
        public string MessageId { get; set; }
    }
}
