using Apps.MicrosoftTeams.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Models.Responses
{
    public class GetLastMessages
    {
        public IEnumerable<MessageDto> Messages { get; set; }
    }
}
