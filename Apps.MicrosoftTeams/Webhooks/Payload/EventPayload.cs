using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.Webhooks.Payload;

public class EventPayload
{
    public string SubscriptionId { get; set; }
    public string ChangeType { get; set; }
    public string Resource { get; set; }
    public ResourceData ResourceData { get; set; }
}

public class ResourceData
{
    public string Id { get; set; }

    [JsonProperty("@odata.id")]
    public string Endpoint { get; set; }
}