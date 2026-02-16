namespace Apps.MicrosoftTeams.Constants;

public static class ConnectionTypes
{
    public const string OAuth = "OAuth";
    public const string OAuthAzure = "OAuthAzure";
    public const string ClientCreds = "ClientCredentials";

    public static readonly IEnumerable<string> SupportedConnectionTypes = [OAuth, OAuthAzure, ClientCreds];
}
