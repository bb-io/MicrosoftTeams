using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;

public class ChannelMessageGetter : ItemGetter<ChannelMessageDto>
{
    public ChannelMessageGetter(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
        : base(authenticationCredentialsProviders) { }

    public override async Task<ChannelMessageDto?> GetItem(EventPayload eventPayload)
    {
        var client = new MSTeamsClient(AuthenticationCredentialsProviders);
        var teamId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "teams");
        var channelId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "channels");
        var message = await client.Teams[teamId].Channels[channelId].Messages[eventPayload.ResourceData.Id].GetAsync();
        return new ChannelMessageDto(message);
    }
}