using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Chat;

public class ChatMessageWithAttachmentsGetter : ItemGetter<ChatMessageDto>
{
    private readonly ChatInput _chat;
    
    public ChatMessageWithAttachmentsGetter(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        ChatInput chat) : base(authenticationCredentialsProviders)
    {
        _chat = chat;
    }

    public override async Task<ChatMessageDto?> GetItem(EventPayload eventPayload)
    {
        var chatId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "chats");

        if (_chat.ChatId is not null && _chat.ChatId != chatId)
            return null;
        
        var client = new MSTeamsClient(AuthenticationCredentialsProviders);
        var message = await client.Me.Chats[chatId].Messages[eventPayload.ResourceData.Id].GetAsync();

        if (!message.Attachments.Any(a => a.ContentType == "reference"))
            return null;

        return new ChatMessageDto(message);
    }
}