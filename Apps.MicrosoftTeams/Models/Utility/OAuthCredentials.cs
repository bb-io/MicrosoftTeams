using Apps.MicrosoftTeams.Constants;

namespace Apps.MicrosoftTeams.Models.Utility;

public class OAuthCredentials
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public string AuthorizeUrl { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;

    public static OAuthCredentials GetOAuthCredentials(Dictionary<string, string> values)
    {
        var clientId = values.GetValueOrDefault(CredNames.AzureClientId) ?? ApplicationConstants.ClientId;
        var secret = values.GetValueOrDefault(CredNames.AzureClientSecret) ?? ApplicationConstants.ClientSecret;
        var tenantId = values.GetValueOrDefault(CredNames.AzureTenantId);

        string baseAuthUrl = tenantId is null 
            ? "https://login.microsoftonline.com/common/oauth2/v2.0" 
            : $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0";

        string scopes;
        if (values.GetValueOrDefault(CredNames.ConnectionType) != ConnectionTypes.OAuthMessagesOnly)
        {
            var adminPermission = values.GetValueOrDefault(CredNames.AdminPermissionRequired)?.ToLower() ?? "no";
            scopes = adminPermission == "yes"
                ? ApplicationConstants.FullScope
                : ApplicationConstants.LimitedScope;
        }
        else
            scopes = ApplicationConstants.MessagesOnlyScope;

        return new OAuthCredentials
        {
            ClientId = clientId,
            ClientSecret = secret,
            AuthorizeUrl = $"{baseAuthUrl}/authorize",
            TokenUrl = $"{baseAuthUrl}/token",
            Scopes = scopes
        };
    }
}
