using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams;

public class MSTeamsClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    : GraphServiceClient(GetAuthenticationProvider(authenticationCredentialsProviders))
{
    private static BaseBearerTokenAuthenticationProvider GetAuthenticationProvider(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var token = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;
        var accessTokenProvider = new AccessTokenProvider(token);
        return new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
    }
}