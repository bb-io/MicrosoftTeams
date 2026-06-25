using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.MicrosoftTeams.Connections;

public class ConnectionValidator(InvocationContext invocationContext) : BaseInvocable(invocationContext), IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        var credsList = authenticationCredentialsProviders.ToList();
        var client = new MSTeamsClient(credsList);

        try
        {
            await client.ValidateConnection(cancellationToken: cancellationToken);
            return new ConnectionValidationResponse
            {
                IsValid = true,
                Message = "Success"
            };
        }
        catch (Exception ex)
        {
            var credentialStrings = credsList
                .Select(p => $"{p.KeyName}: {p.Value}")
                .ToList();
            InvocationContext.Logger?.LogError($"[MicrosoftTeamsValidator] Failed to validate connection. Exception: {ex.Message}; Credentials: {string.Join(", ", credentialStrings)}", []);

            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = string.IsNullOrEmpty(ex.Message) ? "Ping failed" : ex.Message
            };
        }
    }
}