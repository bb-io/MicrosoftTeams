using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Utility;
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

        if (message is null)
            return null;

        var hasRefAttachments = message.Attachments?.Any(a => a?.ContentType == "reference") == true;
        var hasInlineImages = HostedContentImageHelper.GetIds(message.Body?.Content).Count > 0;
        if (!hasRefAttachments && !hasInlineImages) return null;

        if (_sender.UserId is not null && message.From?.User?.Id != _sender.UserId)
            return null;

        return new ChannelMessageDto(message, resource.TeamId, resource.ChannelId,
            resource.MessageId, resource.ReplyId);
    }
}
