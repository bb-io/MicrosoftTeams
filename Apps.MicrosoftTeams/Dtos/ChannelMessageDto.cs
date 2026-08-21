using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;
using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.Dtos;

public class ChannelMessageDto
{
    public ChannelMessageDto(ChatMessage message, string? teamId = null, string? channelId = null,
        string? rootMessageId = null, string? replyId = null)
    {
        Id = message.Id ?? string.Empty;
        RootMessageId = rootMessageId ?? message.ReplyToId ?? Id;
        ReplyId = replyId ?? (message.ReplyToId is null ? null : Id);
        Content = message.Body?.Content ?? string.Empty;
        From = message.From?.User?.DisplayName
               ?? message.From?.Application?.DisplayName
               ?? message.From?.Device?.DisplayName
               ?? string.Empty;
        TeamChannelId = JsonConvert.SerializeObject(new TeamChannel
        {
            TeamId = message.ChannelIdentity?.TeamId ?? teamId ?? string.Empty,
            ChannelId = message.ChannelIdentity?.ChannelId ?? channelId ?? string.Empty
        });
    }
        
    [Display("Message ID")]
    public string Id { get; set; }

    [Display("Root message ID")]
    public string RootMessageId { get; set; }

    [Display("Reply ID")]
    public string? ReplyId { get; set; }

    public string Content { get; set; }
        
    public string From { get; set; }
        
    [Display("Channel")]
    public string TeamChannelId { get; set; }
}
