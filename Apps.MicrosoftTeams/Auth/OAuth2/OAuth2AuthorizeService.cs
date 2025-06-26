using Apps.MicrosoftTeams.Constants;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.MicrosoftTeams.Authorization.OAuth2;

public class OAuth2AuthorizeService(InvocationContext invocationContext) : BaseInvocable(invocationContext), IOAuth2AuthorizeService
{
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        string bridgeOauthUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/oauth";
        const string oauthUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
        var adminPermissionRequired = values.First(v => v.Key == CredNames.AdminPermissionRequired).Value.ToLower();
        var requiredScope = adminPermissionRequired == "yes"
            ? ApplicationConstants.FullScope
            : ApplicationConstants.LimitedScope;
        
        var parameters = new Dictionary<string, string>
        {
            { "client_id", ApplicationConstants.ClientId },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
            { "scope", requiredScope },
            { "state", values["state"] },
            { "response_type", "code" },
            { "authorization_url", oauthUrl},
            { "actual_redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString() },
        };
        
        return QueryHelpers.AddQueryString(bridgeOauthUrl, parameters);
    }
}
