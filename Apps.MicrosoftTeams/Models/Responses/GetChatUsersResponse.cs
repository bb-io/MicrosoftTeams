using Apps.MicrosoftTeams.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Models.Responses
{
    public class GetChatUsersResponse
    {
        public IEnumerable<ChatMemberDto> Members { get; set; }
    }
}
