using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Models.Identifiers;

public class ChannelIdentifier
{
    [Display("Channel")]
    [DataSource(typeof(ChannelHandler))]
    public string TeamChannelId { get; set; } // should be deserialized into TeamChannel class
}