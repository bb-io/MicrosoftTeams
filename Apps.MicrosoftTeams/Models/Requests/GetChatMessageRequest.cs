using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetChatMessageRequest
    {
        public string ChatId { get; set; }
        public string MessageId { get; set; }
    }
}
