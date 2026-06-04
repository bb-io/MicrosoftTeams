using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;

public class ChannelMessageWithAttachmentsGetter : ItemGetter<ChannelMessageDto>
{
    private readonly SenderInput _sender;

    public ChannelMessageWithAttachmentsGetter(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, SenderInput sender)
        : base(authenticationCredentialsProviders)
    {
        _sender = sender;
    }

    public override async Task<ChannelMessageDto?> GetItem(EventPayload eventPayload)
    {
        var client = new MSTeamsClient(AuthenticationCredentialsProviders);
        var resource = ChannelMessageResource.Parse(eventPayload);
        var message = await ChannelMessageReader.GetAsync(client, resource);

        var hasRefAttachments = message.Attachments?.Any(a => a?.ContentType == "reference") == true;
        if (!hasRefAttachments) return null;

        if (_sender.UserId is not null && message.From?.User?.Id != _sender.UserId)
            return null;

        return new ChannelMessageDto(message);
    }
}
