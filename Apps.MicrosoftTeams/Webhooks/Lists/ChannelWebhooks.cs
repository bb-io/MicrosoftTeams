using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Handlers.Channel;
using Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.MicrosoftTeams.Webhooks.Lists;

[WebhookList]
public class ChannelWebhooks : BaseWebhookList
{
    public ChannelWebhooks(InvocationContext invocationContext) : base(invocationContext) { }
    
    [Webhook("On message sent to channel", typeof(MessageSentToChannelWebhookHandler), 
        Description = "This webhook is triggered when a message is sent to the channel.")]
    public async Task<WebhookResponse<ChannelMessageDto>> OnMessageSent(WebhookRequest request)
    {
        return await HandleWebhookRequest(request, new ChannelMessageGetter(AuthenticationCredentialsProviders));
    }
    
    [Webhook("On message with attachment sent to channel", typeof(MessageSentToChannelWebhookHandler), 
        Description = "This webhook is triggered when a message is sent to the channel.")]
    public async Task<WebhookResponse<ChannelMessageDto>> OnMessageWithAttachmentSent(WebhookRequest request)
    {
        return await HandleWebhookRequest(request, 
            new ChannelMessageWithAttachmentsGetter(AuthenticationCredentialsProviders));
    }
}