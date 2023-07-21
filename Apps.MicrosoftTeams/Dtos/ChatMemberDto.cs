using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.Dtos
{
    public class ChatMemberDto
    {
        public ChatMemberDto(ConversationMember member)
        {
            Id = member.Id;
            DisplayName = member.DisplayName;
        }

        [Display("Chat member ID")]
        public string Id { get; set; }

        [Display("Display name")]
        public string DisplayName { get; set; }
    }
}
