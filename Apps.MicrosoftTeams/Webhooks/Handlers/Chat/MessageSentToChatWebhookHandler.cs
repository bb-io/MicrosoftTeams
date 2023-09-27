namespace Apps.MicrosoftTeams.Webhooks.Handlers.Chat;

public class MessageSentToChatWebhookHandler : BaseWebhookHandler
{
    private const string SubscriptionEvent = "created";
    
    public MessageSentToChatWebhookHandler() : base(SubscriptionEvent) { }

    protected override string GetResource() => "/me/chats/getAllMessages";
}