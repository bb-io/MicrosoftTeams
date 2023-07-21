using Apps.MicrosoftTeams.Actions;

namespace Apps.MicrosoftTeams.Models.Responses
{
    public class GetLastMessages
    {
        public IEnumerable<MessageDto> Messages { get; set; }
    }
}
