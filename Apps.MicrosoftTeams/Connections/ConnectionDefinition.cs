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
            Name = ConnectionTypes.ClientCreds,
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
        var token = values.First(v => v.Key == "access_token");
        yield return new AuthenticationCredentialsProvider("Authorization", $"{token.Value}");
    }
}