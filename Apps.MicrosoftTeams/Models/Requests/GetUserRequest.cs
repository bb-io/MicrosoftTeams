using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Requests
{
    public class GetUserRequest
    {
        [Display("User ID")]
        public string UserId { get; set; }
    }
}
