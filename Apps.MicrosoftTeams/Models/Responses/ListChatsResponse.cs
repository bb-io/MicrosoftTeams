using Apps.MicrosoftTeams.Dtos;

namespace Apps.MicrosoftTeams.Models.Responses
{
    public class ListChatsResponse
    {
        public IEnumerable<ChatDto> Chats { get; set; }
    }
}
