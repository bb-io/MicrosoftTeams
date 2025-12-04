using System.Text.Json;
using Apps.MicrosoftTeams.Models.Utility;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;

namespace Apps.MicrosoftTeams.Authorization.OAuth2;

public class OAuth2TokenService(InvocationContext invocationContext) : BaseInvocable(invocationContext), IOAuth2TokenService
{
    private const string ExpiresAtKeyName = "expires_at";

    public bool IsRefreshToken(Dictionary<string, string> values)
        => values.TryGetValue(ExpiresAtKeyName, out var expireValue) && DateTime.UtcNow > DateTime.Parse(expireValue);

    public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        var creds = OAuthCredentials.GetOAuthCredentials(values);
        const string grant_type = "refresh_token";

        var bodyParameters = new Dictionary<string, string>
        {
            { "grant_type", grant_type },
            { "refresh_token", values["refresh_token"] },
            { "client_id", creds.ClientId },
            { "client_secret", creds.ClientSecret }
        };

        return await RequestToken(bodyParameters, creds.TokenUrl, cancellationToken);
    }

    public async Task<Dictionary<string, string>> RequestToken(
        string state,
        string code,
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        const string grant_type = "authorization_code";

        var creds = OAuthCredentials.GetOAuthCredentials(values);
        var bodyParameters = new Dictionary<string, string>
        {
            { "grant_type", grant_type },
            { "client_id", creds.ClientId },
            { "client_secret", creds.ClientSecret },
            { "code", code },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
        };

        return await RequestToken(bodyParameters, creds.TokenUrl, cancellationToken);
    }

    public Task RevokeToken(Dictionary<string, string> values)
    {
        throw new NotImplementedException();
    }

    private async Task<Dictionary<string, string>> RequestToken(
        Dictionary<string, string> bodyParameters,
        string tokenUrl,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        using HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        using var httpContent = new FormUrlEncodedContent(bodyParameters);

        try
        {
            using var response = await httpClient.PostAsync(tokenUrl, httpContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                InvocationContext.Logger?.LogError(
                    $"[MicrosoftTeamsOAuth2] Token request failed with status {response.StatusCode}. Response: {responseContent}",
                    []);
                response.EnsureSuccessStatusCode(); // This will throw
            }

            var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)?.ToDictionary(r => r.Key, r => r.Value?.ToString())
                ?? throw new InvalidOperationException($"Invalid response content: {responseContent}");
            var expiresIn = int.Parse(resultDictionary["expires_in"]);
            var expiresAt = utcNow.AddSeconds(expiresIn);
            resultDictionary.Add(ExpiresAtKeyName, expiresAt.ToString());
            return resultDictionary;
        }
        catch (Exception ex)
        {
            var parameters = bodyParameters.ToDictionary(p => p.Key, p => p.Value);
            InvocationContext.Logger?.LogError(
                $"[MicrosoftTeamsOAuth2] Failed to request token. Exception: {ex.Message}; Parameters: {JsonSerializer.Serialize(parameters)}",
                []);

            throw;
        }
    }
}
