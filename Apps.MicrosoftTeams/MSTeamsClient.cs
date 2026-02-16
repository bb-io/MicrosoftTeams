using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams;

public class MSTeamsClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    : GraphServiceClient(GetAuthenticationProvider(authenticationCredentialsProviders))
{
    private static BaseBearerTokenAuthenticationProvider GetAuthenticationProvider(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        try
        {
            var token = authenticationCredentialsProviders.First(p => p.KeyName == "access_token").Value;
            var accessTokenProvider = new AccessTokenProvider(token);
            return new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed in MSTeamsClient: {ex.Message} Stack: {ex.StackTrace}");
        }
    }
}