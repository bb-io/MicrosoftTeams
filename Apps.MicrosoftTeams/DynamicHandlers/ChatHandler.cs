using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.MicrosoftTeams.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;

namespace Apps.MicrosoftTeams.DynamicHandlers
{
    public class ChatHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public ChatHandler(InvocationContext invocationContext) : base(invocationContext)
        {    
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var contextInv = InvocationContext;
            var client = new MSTeamsClient(contextInv.AuthenticationCredentialsProviders);
            var me = new UserActions().GetMe(contextInv.AuthenticationCredentialsProviders);
            var chats = await client.Me.Chats.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "members" };
                if (!string.IsNullOrEmpty(context.SearchString))
                {
                    var filter = $"contains(topic, '{context.SearchString}') or (members/any(x:contains(x/displayName, '{context.SearchString}')) and chatType eq 'oneOnOne')";
                    requestConfiguration.QueryParameters.Filter = filter;
                }
            });
            return chats.Value.ToDictionary(k => k.Id, v => string.IsNullOrEmpty(v.Topic) ? v.Members.FirstOrDefault(m => m.Id != me.Id.ToString()).DisplayName : v.Topic);
        }
    }
}
