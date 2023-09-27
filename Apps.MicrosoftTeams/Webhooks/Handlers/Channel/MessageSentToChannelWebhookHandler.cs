using Apps.MicrosoftTeams.DynamicHandlers;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.Webhooks.Handlers.Channel;

public class MessageSentToChannelWebhookHandler : BaseWebhookHandler
{
    private const string SubscriptionEvent = "created";
    
    public MessageSentToChannelWebhookHandler([WebhookParameter(true)] ChannelInput channel) 
        : base(SubscriptionEvent, channel) { }

    protected override string GetResource()
    {
        var channel = (ChannelInput)WebhookInput;
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channel.TeamChannelId);
        var resource = $"/teams/{teamChannel.TeamId}/channels/{teamChannel.ChannelId}/messages";
        return resource;
    }
}