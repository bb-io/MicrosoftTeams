using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.DynamicHandlers
{
    public class ChatHandler(InvocationContext invocationContext)
        : BaseInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var logger = new Logger();
            var contextInv = InvocationContext;
            var client = new MSTeamsClient(contextInv.AuthenticationCredentialsProviders);
            var me = await client.Me.GetAsync(cancellationToken: cancellationToken);

            var allChats = new List<Chat>();
            var top = 100;

            var chats = await client.Me.Chats.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Expand = new[] { "members" };
                var filter = $"NOT(chatType eq 'meeting') and ((contains(topic, '{context.SearchString ?? ""}') or " +
                             $"(topic eq null and (members/any(x:contains(x/displayName, '{context.SearchString ?? ""}'))))))";
                requestConfiguration.QueryParameters.Filter = filter;
                requestConfiguration.QueryParameters.Orderby = new[] { "lastMessagePreview/createdDateTime desc" };
                requestConfiguration.QueryParameters.Top = top;
            }, cancellationToken);

            var maxIterations = 4;
            while (chats?.Value != null && maxIterations > 0)
            {
                allChats.AddRange(chats.Value);

                if (chats.Value.Count == 0)
                {
                    break;
                }

                if (!string.IsNullOrEmpty(chats.OdataNextLink))
                {
                    chats = await client.Me.Chats
                        .WithUrl(chats.OdataNextLink)
                        .GetAsync(cancellationToken: cancellationToken);
                    maxIterations--;
                }
                else
                {
                    break;
                }
            }

            return allChats.ToDictionary(k => k.Id, v => string.IsNullOrEmpty(v.Topic)
                ? v.ChatType == ChatType.OneOnOne
                    ? v.Members.FirstOrDefault(m => ((AadUserConversationMember)m).UserId != me.Id)?.DisplayName ?? "Unknown user"
                    : string.Join(", ", v.Members.Where(m => ((AadUserConversationMember)m).UserId != me.Id).Select(m => m.DisplayName))
                : v.Topic);
        }
    }
}