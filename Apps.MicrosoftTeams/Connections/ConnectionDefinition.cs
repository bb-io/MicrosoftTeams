using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.MicrosoftTeams.Connections;

public class ConnectionDefinition : IConnectionDefinition
{

    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
    {
        new()
        {
            Name = "OAuth",
            AuthenticationType = ConnectionAuthenticationType.OAuth2,
            ConnectionUsage = ConnectionUsage.Actions,
            ConnectionProperties = new List<ConnectionProperty>
            {
                new("AdminPermissionRequired") { DisplayName = "Channel messages scope required" }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        var token = values.First(v => v.Key == "access_token");
        yield return new AuthenticationCredentialsProvider(
            AuthenticationCredentialsRequestLocation.None,
            "Authorization",
            $"{token.Value}"
        );
    }
}