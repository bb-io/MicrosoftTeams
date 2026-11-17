using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.Models.Identifiers;

public class ChannelIdentifier
{
    [Display("Channel"), DataSource(typeof(ChannelHandler))]
    public string TeamChannelId { get; set; } = string.Empty;

    public TeamChannel Resolve()
    {
        var misconfigException = new PluginMisconfigurationException("Please select a channel directly from the dynamic dropdown");
        try
        {
            return JsonConvert.DeserializeObject<TeamChannel>(TeamChannelId) ?? throw misconfigException;
        }
        catch (JsonException)
        {
            throw misconfigException;
        }
    }
}