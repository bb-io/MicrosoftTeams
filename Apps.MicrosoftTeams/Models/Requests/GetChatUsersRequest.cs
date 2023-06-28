using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetChatUsersRequest
    {
        [Display("Chat ID")]
        public string ChatId { get; set; }
    }
}
