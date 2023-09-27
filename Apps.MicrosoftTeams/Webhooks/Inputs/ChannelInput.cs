using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Webhooks.Inputs;

public class ChannelInput : IWebhookHandlerInput
{
    [Display("Channel")]
    [DataSource(typeof(ChannelHandler))]
    public string TeamChannelId { get; set; } // should be deserialized into TeamChannel class
}