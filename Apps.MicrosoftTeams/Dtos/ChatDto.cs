using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Dtos
{
    public class ChatDto
    {
        [Display("Chat ID")]
        public string Id { get; set; }

        public string Topic { get; set; }
    }
}
