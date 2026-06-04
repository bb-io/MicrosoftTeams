using Apps.MicrosoftTeams.Constants;
using Apps.MicrosoftTeams.Models.Utility;

namespace Tests.MicrosoftTeams;

[TestClass]
public class OAuthCredentialsTests
{
    [TestMethod]
    public void GetOAuthCredentials_WithAzureCustomScopesConnection_ReturnsCorrectCreds()
    {
        // Arrange
        string clientId = "client_id";
        string clientSecret = "client_secret";
        string tenantId = "tenant_id";
        string scopes = "User.Read";
        
        var values = new Dictionary<string, string>
        {
            { CredNames.AzureClientId, clientId },
            { CredNames.AzureClientSecret, clientSecret },
            { CredNames.AzureTenantId, tenantId },
            { CredNames.CustomScopes, scopes },
            { CredNames.ConnectionType, ConnectionTypes.OAuthAzureCustomScopes }
        };

        // Act
        var creds = OAuthCredentials.GetOAuthCredentials(values);
        
        // Assert
        Assert.IsNotNull(creds);
        Assert.AreEqual(creds.ClientId, clientId);
        Assert.AreEqual(creds.ClientSecret, clientSecret);
        Assert.AreEqual(creds.Scopes, scopes);
    }
}