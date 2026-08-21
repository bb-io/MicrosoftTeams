using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Identifiers;

public class ChannelMessageIdentifier
{
    [Display("Root message ID")]
    public string MessageId { get; set; }

    [Display("Reply ID")]
    public string? ReplyId { get; set; }
}
