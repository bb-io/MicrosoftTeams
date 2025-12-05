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

        string baseAuthUrl;
        if (tenantId is null)
            baseAuthUrl = "https://login.microsoftonline.com/common/oauth2/v2.0";
        else
            baseAuthUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0";

        var adminPermission = values.GetValueOrDefault(CredNames.AdminPermissionRequired)?.ToLower() ?? "no";
        var scopes = adminPermission == "yes"
            ? ApplicationConstants.FullScope
            : ApplicationConstants.LimitedScope;

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
