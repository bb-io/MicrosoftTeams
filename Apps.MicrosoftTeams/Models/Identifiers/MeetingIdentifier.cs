

using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Models.Identifiers
{
    public class MeetingIdentifier
    {
        [Display("Meeting ID")]
        [DataSource(typeof(MeetingHandler))]
        public string Id { get; set; }

    }
}
