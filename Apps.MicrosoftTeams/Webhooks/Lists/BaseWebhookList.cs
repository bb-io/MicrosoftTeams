using System.Net;
using Apps.MicrosoftTeams.Webhooks.Lists.ItemGetters;
using Apps.MicrosoftTeams.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.Webhooks.Lists;

public class BaseWebhookList : BaseInvocable
{
    protected readonly IEnumerable<AuthenticationCredentialsProvider> AuthenticationCredentialsProviders;

    protected BaseWebhookList(InvocationContext invocationContext) : base(invocationContext)
    {
        AuthenticationCredentialsProviders = invocationContext.AuthenticationCredentialsProviders;
    }
    
    protected async Task<WebhookResponse<T>> HandleWebhookRequest<T>(WebhookRequest request,
        ItemGetter<T> itemGetter) where T: class
    {
        var eventPayload = JsonConvert.DeserializeObject<EventPayload>(request.Body.ToString(), new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore
        });
        var item = await itemGetter.GetItem(eventPayload);
        
        if (item is null)
            return new WebhookResponse<T>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };

        return new WebhookResponse<T>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = item
        };
    }
}