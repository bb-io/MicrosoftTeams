using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class SendMessageToChatRequest
    {
        [Display("Chat ID")]
        [DataSource(typeof(ChatHandler))]
        public string ChatId { get; set; }

        public string Message { get; set; }
    }
}
