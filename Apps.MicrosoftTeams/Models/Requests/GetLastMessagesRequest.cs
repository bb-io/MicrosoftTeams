using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetLastMessagesRequest
    {
        [Display("Chat ID")]
        [DataSource(typeof(ChatHandler))]
        public string ChatId { get; set; }
        
        [Display("Messages amount")]
        public int MessagesAmount { get; set; }
    }
}
