using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetLastMessagesRequest
    {
        public string ChatId { get; set; }
        public int MessagesAmount { get; set; }
    }
}
