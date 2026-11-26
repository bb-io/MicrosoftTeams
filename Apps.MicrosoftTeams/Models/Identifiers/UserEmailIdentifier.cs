using Blackbird.Applications.Sdk.Common;

namespace Apps.MicrosoftTeams.Models.Identifiers;

public class UserEmailIdentifier
{
    [Display("Email")]
    public string Email { get; set; } = string.Empty;
}
