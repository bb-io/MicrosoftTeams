using Apps.MicrosoftTeams.Actions;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Microsoft.Kiota.Abstractions;
using RestSharp;
using System.Text.Json;
using System.Threading;

namespace Apps.MicrosoftTeams.Authorization.OAuth2
{
    public class OAuth2TokenService : IOAuth2TokenService
    {
        private const string TokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";

        public bool IsRefreshToken(Dictionary<string, string> values)
        {
            return false;
        }

        public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, string?>> RequestToken(
            string state, 
            string code, 
            Dictionary<string, string> values, 
            CancellationToken cancellationToken)
        {
            

            const string grant_type = "authorization_code";

            var bodyParameters = new Dictionary<string, string>
            {
                { "grant_type", grant_type },
                { "client_id", ApplicationConstants.ClientId },
                { "client_secret", ApplicationConstants.ClientSecret },
                { "code", code },
                { "redirect_uri", ApplicationConstants.RedirectUri },
            };
            return await RequestToken(bodyParameters, cancellationToken);
        }

        public Task RevokeToken(Dictionary<string, string> values)
        {
            throw new NotImplementedException();
        }

        private async Task<Dictionary<string, string>> RequestToken(Dictionary<string, string> bodyParameters, CancellationToken cancellationToken)
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            using var httpContent = new FormUrlEncodedContent(bodyParameters);
            using var response = await httpClient.PostAsync(TokenUrl, httpContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();
            var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)?.ToDictionary(r => r.Key, r => r.Value?.ToString())
                ?? throw new InvalidOperationException($"Invalid response content: {responseContent}");
            return resultDictionary;
        }
    }
}
