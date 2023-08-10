using Apps.MicrosoftTeams.Actions;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.DynamicHandlers
{
    public class UserHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public UserHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var contextInv = InvocationContext;
            var client = new MSTeamsClient(contextInv.AuthenticationCredentialsProviders);
            var users = await client.Users.GetAsync((requestConfiguration) =>
            {
                if (!string.IsNullOrEmpty(context.SearchString))
                {
                    requestConfiguration.QueryParameters.Search = $"\"displayName:{context.SearchString}\"";
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                }
            });
            return users.Value.ToDictionary(k => k.Id, v => v.DisplayName);
        }
    }
}
