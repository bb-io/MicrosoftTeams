namespace Apps.MicrosoftTeams.Constants;

public static class ConnectionTypes
{
    public const string OAuth = "OAuth";
    public const string OAuthAzure = "OAuthAzure";

    public static readonly IEnumerable<string> SupportedConnectionTypes = [OAuth, OAuthAzure];
}
