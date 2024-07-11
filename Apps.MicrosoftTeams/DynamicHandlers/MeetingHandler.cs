using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.MicrosoftTeams.Models.Identifiers;

namespace Apps.MicrosoftTeams.DynamicHandlers
{
    public class MeetingHandler(InvocationContext invocationContext, [ActionParameter] UserIdentifier user)
        : BaseInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var client = new MSTeamsClient(InvocationContext.AuthenticationCredentialsProviders);
            var users = await client.Users[user.UserId].OnlineMeetings.GetAsync(requestConfiguration =>
            {
                if (!string.IsNullOrWhiteSpace(context.SearchString))
                {
                    requestConfiguration.QueryParameters.Search = $"\"subject:{context.SearchString}\"";
                }
            }, cancellationToken);
            return users.Value.ToDictionary(k => k.Id, v => v.Subject);
        }
    }
}
