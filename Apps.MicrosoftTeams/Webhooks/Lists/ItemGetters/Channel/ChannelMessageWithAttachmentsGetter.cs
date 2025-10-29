using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;
using System.Text.RegularExpressions;

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
        var endpoint = eventPayload.ResourceData?.Endpoint
                   ?? eventPayload.Resource
                   ?? throw new InvalidOperationException("No resource endpoint in payload");

        var (teamId, channelId, messageId, replyId) = ParseResource(endpoint);

        Microsoft.Graph.Models.ChatMessage message;
        if (!string.IsNullOrEmpty(replyId))
        {
            message = await client.Teams[teamId].Channels[channelId].Messages[messageId].Replies[replyId].GetAsync();
        }
        else
        {
            message = await client.Teams[teamId].Channels[channelId].Messages[messageId].GetAsync();
        }

        var hasRefAttachments = message.Attachments?.Any(a => a?.ContentType == "reference") == true;
        if (!hasRefAttachments) return null;

        if (_sender.UserId is not null && message.From?.User?.Id != _sender.UserId)
            return null;

        return new ChannelMessageDto(message);
    }

    private static readonly Regex ResourceRx = 
        new(@"teams\('(?<team>[^']+)'\)/channels\('(?<channel>[^']+)'\)/messages\('(?<msg>[^']+)'\)(?:/replies\('(?<reply>[^']+)'\))?",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static (string teamId, string channelId, string messageId, string? replyId) ParseResource(string endpoint)
    {
        var m = ResourceRx.Match(endpoint);
        if (!m.Success) throw new InvalidOperationException($"Cannot parse resource: {endpoint}");
        var reply = m.Groups["reply"].Success ? m.Groups["reply"].Value : null;
        return (m.Groups["team"].Value, m.Groups["channel"].Value, m.Groups["msg"].Value, reply);
    }
}