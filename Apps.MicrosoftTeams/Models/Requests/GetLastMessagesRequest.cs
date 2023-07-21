using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetLastMessagesRequest
    {
        [Display("Chat ID")]
        public string ChatId { get; set; }
        
        [Display("Messages amount")]
        public int MessagesAmount { get; set; }
    }
}
