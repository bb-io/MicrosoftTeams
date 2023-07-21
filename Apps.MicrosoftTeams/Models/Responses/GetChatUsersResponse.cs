using Apps.MicrosoftTeams.Dtos;

namespace Apps.MicrosoftTeams.Models.Responses
{
    public class GetChatUsersResponse
    {
        public IEnumerable<ChatMemberDto> Members { get; set; }
    }
}
