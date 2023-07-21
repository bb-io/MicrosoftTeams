using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Actions
{
    public class MessageDto
    {
        [Display("Message ID")]
        public string? Id { get; set; }

        public string? Content { get; set; }
    }
}
