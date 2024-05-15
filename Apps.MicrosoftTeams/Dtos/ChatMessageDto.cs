using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.Dtos;

public class ChatMessageDto
{
    public ChatMessageDto(ChatMessage message)
    {
        Id = message.Id;
        Content = message.Body.Content;
        From = message.From.User.DisplayName;
        ChatId = message.ChatId;
    }
        
    [Display("Message ID")]
    public string Id { get; set; }

    public string Content { get; set; }
        
    public string From { get; set; }
        
    [Display("Chat")]
    public string ChatId { get; set; }
}