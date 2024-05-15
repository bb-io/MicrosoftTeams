using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;

/*
                 var pageIterator = Microsoft.Graph.PageIterator<Chat, ChatCollectionResponse>.CreatePageIterator(client, chatsResponse, (m) =>
                {
                    count++;
                    if (count < 1000)
                    {
                        return false;
                    }

                    return true;
                });

                await pageIterator.IterateAsync(cancellationToken);
 */

namespace Apps.MicrosoftTeams.DynamicHandlers
{
    public class ChatHandler(InvocationContext invocationContext)
        : BaseInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
            CancellationToken cancellationToken)
        {
            var logger = new Logger();
            var iteration = 0;
            const int maxIterations = 5;
            const int top = 50;

            try
            {
                var contextInv = InvocationContext;
                var client = new MSTeamsClient(contextInv.AuthenticationCredentialsProviders);

                var me = await client.Me.GetAsync(cancellationToken: cancellationToken);
                var allChats = new List<Chat>();

                var filter = $"NOT(chatType eq 'meeting') and ((contains(topic, '{context.SearchString ?? ""}') or " +
                             $"(topic eq null and (members/any(x:contains(x/displayName, '{context.SearchString ?? ""}'))))))";

                var chatsResponse = await client.Me.Chats.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Expand = new[] { "members" };
                    requestConfiguration.QueryParameters.Filter = filter;
                    requestConfiguration.QueryParameters.Orderby = new[] { "lastMessagePreview/createdDateTime desc" };
                    requestConfiguration.QueryParameters.Count = true;
                    requestConfiguration.QueryParameters.Top = top;
                }, cancellationToken);

                var nextPageLink = chatsResponse?.OdataNextLink;

                while (!string.IsNullOrEmpty(nextPageLink) && iteration < maxIterations)
                {
                    await logger.Log(new
                    {
                        Message = "Getting next page",
                        Iteration = iteration,
                        NextPageLink = nextPageLink
                    });

                    var nextPageRequestInformation = new RequestInformation
                    {
                        HttpMethod = Method.GET,
                        UrlTemplate = nextPageLink,
                    };

                    var nextPageResult = await client.RequestAdapter.SendAsync(nextPageRequestInformation,
                        parseNode => new ChatCollectionResponse(), cancellationToken: cancellationToken);

                    if (nextPageResult?.Value == null || !nextPageResult.Value.Any())
                    {
                        break;
                    }

                    allChats.AddRange(nextPageResult.Value);

                    nextPageLink = nextPageResult.OdataNextLink;
                    iteration++;

                    await logger.Log(new
                    {
                        Message = "Got next page",
                        Iteration = iteration,
                        NextPageResult = nextPageResult.Value
                    });
                }

                return allChats.ToDictionary(k => k.Id, v => string.IsNullOrEmpty(v.Topic)
                    ? v.ChatType == ChatType.OneOnOne
                        ? v.Members.FirstOrDefault(m => ((AadUserConversationMember)m).UserId != me.Id)?.DisplayName ??
                          "Unknown user"
                        : string.Join(", ",
                            v.Members.Where(m => ((AadUserConversationMember)m).UserId != me.Id)
                                .Select(m => m.DisplayName))
                    : v.Topic);
            }
            catch (Exception e)
            {
                await logger.Log(new
                {
                    Message = "Failed to get chats",
                    ExceptionType = e.GetType().Name,
                    ExceptionMessage = e.Message,
                    Iteration = iteration,
                    StackTrace = e.StackTrace
                });

                throw new Exception(
                    $"Failed to get chats, Exception type: {e.GetType().Name}, Message: {e.Message}; Iteration: {iteration}",
                    e);
            }
        }
    }
}