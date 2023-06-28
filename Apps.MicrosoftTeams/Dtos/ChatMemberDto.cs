using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Dtos
{
    public class ChatMemberDto
    {
        public ChatMemberDto(ConversationMember member)
        {
            Id = member.Id;
            DisplayName = member.DisplayName;
        }

        public string Id { get; set; }

        public string DisplayName { get; set; }
    }
}
