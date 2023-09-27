using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Webhooks.Inputs;

public class ChatInput
{
    [Display("Chat")]
    [DataSource(typeof(ChatHandler))]
    public string? ChatId { get; set; }
}