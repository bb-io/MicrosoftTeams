using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;

public class ChannelMessageWithAttachmentsGetter : ItemGetter<ChannelMessageDto>
{
    public ChannelMessageWithAttachmentsGetter(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) 
        : base(authenticationCredentialsProviders) { }

    public override async Task<ChannelMessageDto?> GetItem(EventPayload eventPayload)
    {
        var client = new MSTeamsClient(AuthenticationCredentialsProviders);
        var teamId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "teams");
        var channelId = GetIdFromEndpoint(eventPayload.ResourceData.Endpoint, "channels");
        var message = await client.Teams[teamId].Channels[channelId].Messages[eventPayload.ResourceData.Id].GetAsync();
        
        if (!message.Attachments.Any(a => a.ContentType == "reference"))
            return null;
        
        return new ChannelMessageDto(message);
    }
}