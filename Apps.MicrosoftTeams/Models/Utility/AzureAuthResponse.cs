using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.Models.Utility;

public class AzureAuthResponse
{
    [JsonProperty("access_token")]
    public required string AccessToken { get; set; }
}
