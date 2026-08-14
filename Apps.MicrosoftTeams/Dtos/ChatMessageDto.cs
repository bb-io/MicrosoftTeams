using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.Dtos;

public class ChatMessageDto
{
    public ChatMessageDto(ChatMessage message)
    {
        Id = message.Id ?? string.Empty;
        Content = message.Body?.Content ?? string.Empty;
        From = message.From?.User?.DisplayName
               ?? message.From?.Application?.DisplayName
               ?? message.From?.Device?.DisplayName
               ?? string.Empty;
        ChatId = message.ChatId ?? string.Empty;
    }
        
    [Display("Message ID")]
    public string Id { get; set; }

    public string Content { get; set; }
        
    public string From { get; set; }
        
    [Display("Chat")]
    public string ChatId { get; set; }
}