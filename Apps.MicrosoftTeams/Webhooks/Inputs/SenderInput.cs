using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Webhooks.Inputs;

public class SenderInput
{
    [Display("Sender")]
    [DataSource(typeof(UserHandler))]
    public string? UserId { get; set; }
}