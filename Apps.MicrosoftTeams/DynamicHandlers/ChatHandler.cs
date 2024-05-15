using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Graph;
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
            var contextInv = InvocationContext;
            var client = new MSTeamsClient(contextInv.AuthenticationCredentialsProviders);
            var me = await client.Me.GetAsync(cancellationToken: cancellationToken);

            var chatsResponse = new ChatCollectionResponse() { Value = new List<Chat>() };
            var count = 0;
            var pageIterator = Microsoft.Graph.PageIterator<Chat, ChatCollectionResponse>.CreatePageIterator(client, chatsResponse, async (m) =>
            {
                count++;

                if (count > 100)
                {
                    return false;
                }

                await logger.Log(new
                {
                    Iteration = count,
                    chatId = m
                });
                
                if(m != null)
                {
                    return true;
                }
                
                return false;
            });

            while (pageIterator.State != PagingState.Complete)
            {
                count = 0;
                
                if(pageIterator.State == PagingState.NotStarted)
                {
                    await pageIterator.IterateAsync(cancellationToken);
                }
                else
                {
                    await pageIterator.ResumeAsync(cancellationToken);                
                }
            }
            
            return chatsResponse.Value
                .ToDictionary(k => k.Id, v => string.IsNullOrEmpty(v.Topic) 
                    ? v.ChatType == ChatType.OneOnOne 
                        ? v.Members.FirstOrDefault(m => ((AadUserConversationMember)m).UserId != me.Id)?.DisplayName ?? "Unknown user"
                        : string.Join(", ", v.Members.Where(m => ((AadUserConversationMember)m).UserId != me.Id)
                            .Select(m => m.DisplayName)) 
                    : v.Topic);
        }
    }
}