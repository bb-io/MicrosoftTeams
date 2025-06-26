using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Handlers.Channel;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.MicrosoftTeams.Webhooks.Lists;

[WebhookList]
public class ChannelWebhooks(InvocationContext invocationContext) : BaseWebhookList(invocationContext)
{
    [Webhook("On message sent to channel", typeof(MessageSentToChannelWebhookHandler), 
        Description = "This webhook is triggered when a message is sent to the channel.")]
    public async Task<WebhookResponse<ChannelMessageDto>> OnMessageSent(WebhookRequest request,
        [WebhookParameter] SenderInput sender)
    {
        return await HandleWebhookRequest(request, 
            new ChannelMessageWithSenderGetter(AuthenticationCredentialsProviders, sender));
    }
    
    [Webhook("On message with attachments sent to channel", typeof(MessageSentToChannelWebhookHandler), 
        Description = "This webhook is triggered when a message with attachments is sent to the channel.")]
    public async Task<WebhookResponse<ChannelMessageDto>> OnMessageWithAttachmentSent(WebhookRequest request,
        [WebhookParameter] SenderInput sender)
    {
        return await HandleWebhookRequest(request, 
            new ChannelMessageWithAttachmentsGetter(AuthenticationCredentialsProviders, sender));
    }
    
    [Webhook("On user mentioned in channel", typeof(MessageSentToChannelWebhookHandler), 
        Description = "This webhook is triggered when a new message is sent to the channel with specified user mentioned.")]
    public async Task<WebhookResponse<ChannelMessageDto>> OnUserMentioned(WebhookRequest request, 
        [WebhookParameter] UserInput user)
    {
        return await HandleWebhookRequest(request, 
            new ChannelMessageWithUserMentionedGetter(AuthenticationCredentialsProviders, user));
    }
}