using Apps.MicrosoftTeams.Constants;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
    {
        new()
        {
            Name = ConnectionTypes.OAuth,
            DisplayName = "OAuth2",
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionProperties =
            [
                new(CredNames.AdminPermissionRequired)
                {
                    DisplayName = "Channel messages scope required",
                    DataItems = 
                    [
                        new("yes", "Yes"),
                        new("no", "No")
                    ]
                }
            ]
        },
        new()
        {
            Name = ConnectionTypes.OAuthAzure,
            DisplayName = "OAuth2 (Azure app)",
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionProperties =
            [
                new(CredNames.AdminPermissionRequired)
                {
                    DisplayName = "Channel messages scope required",
                    DataItems =
                    [
                        new("yes", "Yes"),
                        new("no", "No")
                    ]
                },
                new(CredNames.AzureClientId) { DisplayName = "Application (client) ID" },
                new(CredNames.AzureTenantId) { DisplayName = "Directory (tenant) ID" },
                new(CredNames.AzureClientSecret) { DisplayName = "Client secret", Sensitive = true }
            ]
        },
        new()
        {
            Name = ConnectionTypes.AzureAppCreds,
            DisplayName = "Service Account (Client Credentials)",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties =
            [
                new(CredNames.AzureClientId) { DisplayName = "Application (client) ID" },
                new(CredNames.AzureTenantId) { DisplayName = "Directory (tenant) ID" },
                new(CredNames.AzureClientSecret) { DisplayName = "Client secret", Sensitive = true }
            ]
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        try
        {
            var credentials = values.Select(x => new AuthenticationCredentialsProvider(x.Key, x.Value)).ToList();
            var connectionType = values[nameof(ConnectionPropertyGroup)] switch
            {
                var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
                _ => throw new Exception($"Unknown connection type: {values[nameof(ConnectionPropertyGroup)]}")
            };

            credentials.Add(new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType));
            return credentials;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed in CreateAuthorizationCredentialsProviders: {ex.Message} Stack: {ex.StackTrace}");
        }
    }
}