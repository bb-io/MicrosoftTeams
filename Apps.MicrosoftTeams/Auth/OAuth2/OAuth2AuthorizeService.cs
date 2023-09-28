using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.MicrosoftTeams.Authorization.OAuth2
{
    public class OAuth2AuthorizeService : IOAuth2AuthorizeService
    {
        public string GetAuthorizationUrl(Dictionary<string, string> values)
        {
            const string oauthUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
            var adminPermissionRequired = values.First(v => v.Key == "AdminPermissionRequired").Value.ToLower();
            var requiredScope = adminPermissionRequired == "yes"
                ? ApplicationConstants.FullScope
                : ApplicationConstants.LimitedScope;
            
            var parameters = new Dictionary<string, string>
            {
                { "client_id", ApplicationConstants.ClientId },
                { "redirect_uri", ApplicationConstants.RedirectUri },
                { "scope", requiredScope },
                { "state", values["state"] },
                { "response_type", "code" }
            };
            return QueryHelpers.AddQueryString(oauthUrl, parameters);
        }
    }
}
