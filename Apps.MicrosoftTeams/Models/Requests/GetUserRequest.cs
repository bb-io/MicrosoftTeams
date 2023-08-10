using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetUserRequest
    {
        [Display("User ID")]
        //[DataSource(typeof(UserHandler))]
        public string UserId { get; set; }
    }
}
