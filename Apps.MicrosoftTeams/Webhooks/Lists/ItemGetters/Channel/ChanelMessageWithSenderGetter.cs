using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;

public class ChannelMessageWithSenderGetter : ItemGetter<ChannelMessageDto>
{
    private readonly SenderInput _sender;
    private readonly MessageContainsInput _messageFilter;

    public ChannelMessageWithSenderGetter(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        SenderInput sender,MessageContainsInput messageFilter)
        : base(authenticationCredentialsProviders)
    {
        _sender = sender;
        _messageFilter = messageFilter;
    }

    public override async Task<ChannelMessageDto?> GetItem(EventPayload eventPayload)
    {
        var client = new MSTeamsClient(AuthenticationCredentialsProviders);
        var resource = ChannelMessageResource.Parse(eventPayload);
        var message = await ChannelMessageReader.GetAsync(client, resource);

        if (message is null)
            return null;

        if (_sender.UserId is not null && _sender.UserId != message.From?.User?.Id)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_messageFilter.Contains)
            && message.Body?.Content?.Contains(_messageFilter.Contains, StringComparison.OrdinalIgnoreCase) != true)
            return null;

        return new ChannelMessageDto(message, resource.TeamId, resource.ChannelId,
            resource.MessageId, resource.ReplyId);
    }
}
