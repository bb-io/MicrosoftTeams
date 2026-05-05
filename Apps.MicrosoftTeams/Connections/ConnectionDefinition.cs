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
                new(CredNames.MessagesOnlyPermissions)
                {
                    DisplayName = "Chats, channels and messages only scopes",
                    DataItems =
                    [
                        new("yes", "Yes"),
                        new("no", "No")
                    ]
                },
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
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        string token = values.First(v => v.Key == "access_token").Value;
        var providers = new List<AuthenticationCredentialsProvider> { new("Authorization", token) };
        
        var connectionType = values[nameof(ConnectionPropertyGroup)] switch
        {
            var ct when ConnectionTypes.SupportedConnectionTypes.Contains(ct) => ct,
            _ => throw new Exception($"Unknown connection type: {values[nameof(ConnectionPropertyGroup)]}")
        };

        providers.Add(new AuthenticationCredentialsProvider(CredNames.ConnectionType, connectionType));
        return providers;
    }
}