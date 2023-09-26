using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Identifiers;

public class UserIdentifier
{
    [Display("User ID")]
    //[DataSource(typeof(UserHandler))]
    public string UserId { get; set; }
}