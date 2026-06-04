using Apps.MicrosoftTeams.Constants;
using Blackbird.Applications.Sdk.Common.Connections;

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
        string connectionType = values[nameof(ConnectionPropertyGroup)] switch
        {
            var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
            _ => throw new Exception(
                $"Unknown connection type in OAuthCredentials class: {values[nameof(ConnectionPropertyGroup)]}")
        };
        
        var clientId = values.GetValueOrDefault(CredNames.AzureClientId) ?? ApplicationConstants.ClientId;
        var secret = values.GetValueOrDefault(CredNames.AzureClientSecret) ?? ApplicationConstants.ClientSecret;
        var tenantId = values.GetValueOrDefault(CredNames.AzureTenantId);

        string baseAuthUrl = tenantId is null 
            ? "https://login.microsoftonline.com/common/oauth2/v2.0" 
            : $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0";

        string scopes;
        bool messagesOnlyScopes = values.GetValueOrDefault(CredNames.MessagesOnlyPermissions)?.ToLower() == "yes";
        bool adminPermission = values.GetValueOrDefault(CredNames.AdminPermissionRequired)?.ToLower() == "yes";

        if (string.Equals(connectionType, ConnectionTypes.OAuthAzureCustomScopes, StringComparison.OrdinalIgnoreCase))
            scopes = values.GetValueOrDefault(CredNames.CustomScopes) ?? ApplicationConstants.FullScope;
        else if (messagesOnlyScopes)
            scopes = ApplicationConstants.MessagesOnlyScope;
        else if (adminPermission)
            scopes = ApplicationConstants.LimitedScope;
        else
            scopes = ApplicationConstants.FullScope;

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
