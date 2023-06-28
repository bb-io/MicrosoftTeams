using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetUserRequest
    {
        [Display("User ID")]
        public string UserId { get; set; }
    }
}
