using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetChatUsersRequest
    {
        [Display("Chat ID")]
        public string ChatId { get; set; }
    }
}
