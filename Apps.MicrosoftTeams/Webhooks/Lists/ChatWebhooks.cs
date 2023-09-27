using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Handlers.Chat;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Chat;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.MicrosoftTeams.Webhooks.Lists;

[WebhookList]
public class ChatWebhooks : BaseWebhookList
{
    public ChatWebhooks(InvocationContext invocationContext) : base(invocationContext) { }
    
    [Webhook("On message sent to chat", typeof(MessageSentToChatWebhookHandler), 
        Description = "This webhook is triggered when a message is sent to the chat.")]
    public async Task<WebhookResponse<ChatMessageDto>> OnMessageSent(WebhookRequest request, 
        [WebhookParameter] ChatInput chat)
    {
        return await HandleWebhookRequest(request, new ChatMessageGetter(AuthenticationCredentialsProviders, chat));
    }
    
    [Webhook("On message with attachments sent to chat", typeof(MessageSentToChatWebhookHandler), 
        Description = "This webhook is triggered when a message with attachments is sent to the chat.")]
    public async Task<WebhookResponse<ChatMessageDto>> OnMessageWithAttachmentsSent(WebhookRequest request, 
        [WebhookParameter] ChatInput chat)
    {
        return await HandleWebhookRequest(request, 
            new ChatMessageWithAttachmentsGetter(AuthenticationCredentialsProviders, chat));
    }
}