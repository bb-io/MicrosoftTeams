using Blackbird.Applications.Sdk.Common.Authentication;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Apps.MicrosoftTeams;

public class MSTeamsClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    : GraphServiceClient(GetAuthenticationProvider(authenticationCredentialsProviders))
{
    private static BaseBearerTokenAuthenticationProvider GetAuthenticationProvider(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        try
        {
            var token = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;
            var accessTokenProvider = new AccessTokenProvider(token);
            return new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed in MSTeamsClient: {ex.Message} Stack: {ex.StackTrace}");
        }
    }
}