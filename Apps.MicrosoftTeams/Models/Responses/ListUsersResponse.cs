using Apps.MicrosoftTeams.Dtos;

namespace Apps.MicrosoftTeams.Models.Responses
{
    public class ListUsersResponse
    {
        public IEnumerable<UserDto> Users { get; set; }
    }
}
