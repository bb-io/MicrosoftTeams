using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;

namespace Apps.MicrosoftTeams.DynamicHandlers
{
    public class ChatHandler(InvocationContext invocationContext)
        : BaseInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var contextInv = InvocationContext;
            var client = new MSTeamsClient(contextInv.AuthenticationCredentialsProviders);
            var me = await client.Me.GetAsync(cancellationToken: cancellationToken);

            var allChats = new List<Chat>();
            const int top = 50;
            
            var chatsResponse = await client.Me.Chats.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Expand = new[] { "members" };
                var filter =
                    $"NOT(chatType eq 'meeting') and ((contains(topic, '{context.SearchString ?? ""}') or " +
                    $"(topic eq null and (members/any(x:contains(x/displayName, '{context.SearchString ?? ""}'))))))";
                requestConfiguration.QueryParameters.Filter = filter;
                requestConfiguration.QueryParameters.Orderby =
                    new[] { "lastMessagePreview/createdDateTime desc" };
                requestConfiguration.QueryParameters.Top = top;
            }, cancellationToken);
            
            var iteration = 0;

            while (true)
            {
                try
                {
                    var nextPageLink = chatsResponse?.OdataNextLink;
                    if(string.IsNullOrEmpty(nextPageLink))
                    {
                        break;
                    }
                    
                    var nextPageRequestInformation = new RequestInformation
                    {
                        HttpMethod = Method.GET,
                        UrlTemplate = nextPageLink
                    };

                    var nextPageResult = await client.RequestAdapter.SendAsync(nextPageRequestInformation, (parseNode) => new ChatCollectionResponse(), cancellationToken: cancellationToken);
                    
                    if(nextPageResult == null || nextPageResult.Value == null || !nextPageResult.Value.Any())
                    {
                        break;
                    }
                    
                    iteration++;
                    allChats.AddRange(nextPageResult?.Value ?? Enumerable.Empty<Chat>());
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to get chats, Exception type: {e.GetType().Name}, Message: {e.Message}; Iteration: {iteration}");
                }
            }

            return allChats
                .ToDictionary(k => k.Id, v => string.IsNullOrEmpty(v.Topic)
                    ? v.ChatType == ChatType.OneOnOne
                        ? v.Members.FirstOrDefault(m => ((AadUserConversationMember)m).UserId != me.Id)?.DisplayName ?? "Unknown user"
                        : string.Join(", ", v.Members.Where(m => ((AadUserConversationMember)m).UserId != me.Id)
                            .Select(m => m.DisplayName))
                    : v.Topic);
        }
    }
}
