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
        var teamId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "teams");
        var channelId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "channels");
        var message = await client.Teams[teamId].Channels[channelId].Messages[eventPayload.ResourceData.Id].GetAsync();

        if (_sender.UserId is not null && _sender.UserId != message.From.User.Id)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_messageFilter.Contains) && !message.Body.Content.Contains(_messageFilter.Contains, StringComparison.OrdinalIgnoreCase))
            return null;


        return new ChannelMessageDto(message);
    }
}