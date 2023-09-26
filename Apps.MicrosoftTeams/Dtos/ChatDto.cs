using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.Dtos
{
    public class ChatDto
    {
        public ChatDto(Chat chat)
        {
            ChatId = chat.Id;
            Topic = chat.Topic;
        }
        
        [Display("Chat")]
        public string ChatId { get; set; }

        public string Topic { get; set; }
    }
}
