using Apps.MicrosoftTeams.Constants;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Microsoft.Graph.Models.ODataErrors;

namespace Apps.MicrosoftTeams;

public class MSTeamsClient(IEnumerable<AuthenticationCredentialsProvider> creds)
    : GraphServiceClient(GetAuthenticationProvider(creds))
{
    public Task ValidateConnection(CancellationToken cancellationToken)
    {
        var scopes = creds.FirstOrDefault(c => c.KeyName == CredNames.CustomScopes)?.Value ?? string.Empty;
        
        Task validation = scopes switch
        {
            _ when string.IsNullOrWhiteSpace(scopes) || Has(scopes, "User.Read")
                => Me.GetAsync(cancellationToken: cancellationToken),
            
            _ when Has(scopes, "Chat")
                => Me.Chats.GetAsync(r => r.QueryParameters.Top = 1, cancellationToken),

            _ when Has(scopes, "Channel") || Has(scopes, "Team")
                => Me.JoinedTeams.GetAsync(cancellationToken: cancellationToken),

            _ => throw new PluginMisconfigurationException(
                "Could not validate the connection. Please ensure the 'User.Read' scope is enabled and inputted")
        };

        return validation;
    }

    public async Task<T> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            throw ConfigureErrorException(ex);
        }
    }
    
    public async Task ExecuteWithErrorHandlingAsync(Func<Task> action)
    {
        try
        { 
            await action();
        }
        catch (Exception ex)
        {
            throw ConfigureErrorException(ex);
        }
    }
    
    public static Exception ConfigureErrorException(Exception ex)
    {
        if (ex is not ODataError oDataEx) 
            return new PluginApplicationException($"An error occurred: {ex.Message}");
        
        string? errorMessage = oDataEx.Error?.Message;
        string exceptionMessage = !string.IsNullOrWhiteSpace(errorMessage) 
            ? errorMessage 
            : "An unknown error occured";
            
        return new PluginApplicationException(exceptionMessage);
    }
    
    private static bool Has(string scopes, string scope) => scopes.Contains(scope, StringComparison.OrdinalIgnoreCase);
    
    private static BaseBearerTokenAuthenticationProvider GetAuthenticationProvider(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var token = authenticationCredentialsProviders.First(p => p.KeyName == "Authorization").Value;
        var accessTokenProvider = new AccessTokenProvider(token);
        
        return new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
    }
}