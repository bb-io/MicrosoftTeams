using Apps.MicrosoftTeams.Constants;
using Apps.MicrosoftTeams.Models.Utility;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.MicrosoftTeams;

public class MSTeamsClient(IEnumerable<AuthenticationCredentialsProvider> creds)
    : GraphServiceClient(GetAuthenticationProvider(creds))
{
    private static BaseBearerTokenAuthenticationProvider GetAuthenticationProvider(
        IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        string connectionType = creds.Get(CredNames.ConnectionType).Value;
        AccessTokenProvider provider;
        switch (connectionType)
        {
            case ConnectionTypes.OAuth:
            case ConnectionTypes.OAuthAzure:
                string oauthToken = creds.First(p => p.KeyName == "access_token").Value;
                provider = new AccessTokenProvider(oauthToken);
                break;
            case ConnectionTypes.AzureAppCreds:
                string azureToken = GetAzureCredsToken(creds);
                provider = new AccessTokenProvider(azureToken);
                break;
            default:
                throw new Exception($"Unsupported connection type in MSTeamsClient: {connectionType}");
        }
        return new BaseBearerTokenAuthenticationProvider(provider);
    }

    private static string GetAzureCredsToken(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        string clientId = creds.Get(CredNames.AzureClientId).Value;
        string tenantId = creds.Get(CredNames.AzureTenantId).Value;
        string clientSecret = creds.Get(CredNames.AzureClientSecret).Value;

        var client = new RestClient($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0");
        var request = new RestRequest("/token", Method.Post);

        request.AddParameter("grant_type", "client_credentials");
        request.AddParameter("client_id", clientId);
        request.AddParameter("client_secret", clientSecret);
        request.AddParameter("scope", "https://graph.microsoft.com/.default");

        var response = client.Execute(request);

        if (!response.IsSuccessful)
            throw new Exception($"Failed to fetch Azure token. Status: {response.StatusCode}. Content: {response.Content}");

        var result = JsonConvert.DeserializeObject<AzureAuthResponse>(response.Content!);
        return result?.AccessToken ?? throw new Exception("Access token missing in response (Azure)");
    }
}