using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Webhooks.Inputs
{
    public class MessageContainsInput
    {
        [Display("Message contains")]
        public string? Contains { get; set; }
    }
}
