using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.MicrosoftTeams.Webhooks.Handlers.Chat;

public class MessageSentToChatWebhookHandler : BaseWebhookHandler
{
    private const string SubscriptionEvent = "created";
    
    public MessageSentToChatWebhookHandler(InvocationContext invocationContext) : base(invocationContext, SubscriptionEvent) { }

    protected override string GetResource() => "/me/chats/getAllMessages";
}