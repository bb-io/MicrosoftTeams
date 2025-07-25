﻿using Apps.MicrosoftTeams.Constants;
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
            ConnectionProperties = new List<ConnectionProperty>
            {
                new(CredNames.AdminPermissionRequired)
                {
                    DisplayName = "Channel messages scope required",
                    DataItems = 
                    [
                        new("yes", "Yes"),
                        new("no", "No")
                    ]
                }
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        var token = values.First(v => v.Key == "access_token");
        yield return new AuthenticationCredentialsProvider("Authorization", $"{token.Value}");
    }
}