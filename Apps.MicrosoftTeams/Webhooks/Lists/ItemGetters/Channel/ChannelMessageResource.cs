using System.Text.RegularExpressions;
using Apps.MicrosoftTeams.Webhooks.Payload;

namespace Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters.Channel;

internal sealed record ChannelMessageResource(string TeamId, string ChannelId, string MessageId, string? ReplyId)
{
    private static readonly Regex ResourceRx =
        new(@"teams\('(?<team>[^']+)'\)/channels\('(?<channel>[^']+)'\)/messages\('(?<msg>[^']+)'\)(?:/replies\('(?<reply>[^']+)'\))?",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static ChannelMessageResource Parse(EventPayload eventPayload)
    {
        var endpoint = eventPayload.ResourceData?.Endpoint
            ?? eventPayload.Resource
            ?? throw new InvalidOperationException("No resource endpoint in payload");

        var match = ResourceRx.Match(endpoint);
        if (!match.Success)
            throw new InvalidOperationException($"Cannot parse channel message resource: {endpoint}");

        var replyId = match.Groups["reply"].Success ? match.Groups["reply"].Value : null;
        return new(
            match.Groups["team"].Value,
            match.Groups["channel"].Value,
            match.Groups["msg"].Value,
            replyId);
    }
}

internal static class ChannelMessageReader
{
    public static async Task<Microsoft.Graph.Models.ChatMessage> GetAsync(MSTeamsClient client, ChannelMessageResource resource)
    {
        if (!string.IsNullOrEmpty(resource.ReplyId))
        {
            return await client.Teams[resource.TeamId]
                .Channels[resource.ChannelId]
                .Messages[resource.MessageId]
                .Replies[resource.ReplyId]
                .GetAsync();
        }

        return await client.Teams[resource.TeamId]
            .Channels[resource.ChannelId]
            .Messages[resource.MessageId]
            .GetAsync();
    }
}
