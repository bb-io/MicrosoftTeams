using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Chat;

public class ChatMessageWithSenderGetter : ItemGetter<ChatMessageDto>
{
    private readonly ChatInput _chat;
    private readonly SenderInput _sender;
    private readonly MessageContainsInput _messageFilter;

    public ChatMessageWithSenderGetter(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        ChatInput chat, SenderInput sender, MessageContainsInput messageFilter) : base(authenticationCredentialsProviders)
    {
        _chat = chat;
        _sender = sender;
        _messageFilter = messageFilter;
    }

    public override async Task<ChatMessageDto?> GetItem(EventPayload eventPayload)
    {
        var chatId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "chats");

        if (_chat.ChatId is not null && _chat.ChatId != chatId)
            return null;
        
        var client = new MSTeamsClient(AuthenticationCredentialsProviders);
        var message = await client.Me.Chats[chatId].Messages[eventPayload.ResourceData.Id].GetAsync();
        
        if (_sender.UserId is not null && _sender.UserId != message.From.User.Id)
            return null;

        if (!string.IsNullOrWhiteSpace(_messageFilter.Contains) && !message.Body.Content.Contains(_messageFilter.Contains,StringComparison.OrdinalIgnoreCase))
            return null;
        

        return new ChatMessageDto(message);
    }
}