using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;

public class ChannelMessageWithUserMentionedGetter : ItemGetter<ChannelMessageDto>
{
    private readonly UserInput _user;

    public ChannelMessageWithUserMentionedGetter(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, UserInput user)
        : base(authenticationCredentialsProviders)
    {
        _user = user;
    }

    public override async Task<ChannelMessageDto?> GetItem(EventPayload eventPayload)
    {
        var client = new MSTeamsClient(AuthenticationCredentialsProviders);
        var resource = ChannelMessageResource.Parse(eventPayload);
        var message = await ChannelMessageReader.GetAsync(client, resource);

        if (!message.Mentions.Any(user => user.Mentioned.User.Id == _user.UserId))
            return null;
        
        return new ChannelMessageDto(message);
    }
}
