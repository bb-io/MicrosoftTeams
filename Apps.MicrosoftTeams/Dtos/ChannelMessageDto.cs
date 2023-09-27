using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;
using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.Dtos;

public class ChannelMessageDto
{
    public ChannelMessageDto(ChatMessage message)
    {
        Id = message.Id;
        Content = message.Body.Content;
        From = message.From.User.DisplayName;
        TeamChannelId = JsonConvert.SerializeObject(new TeamChannel 
            { TeamId = message.ChannelIdentity.TeamId, ChannelId = message.ChannelIdentity.ChannelId });
    }
        
    [Display("Message ID")]
    public string Id { get; set; }

    public string Content { get; set; }
        
    public string From { get; set; }
        
    [Display("Channel")]
    public string TeamChannelId { get; set; }
}