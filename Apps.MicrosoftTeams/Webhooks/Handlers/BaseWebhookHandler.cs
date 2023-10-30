using Apps.MicrosoftTeams.Webhooks.Inputs;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.Webhooks.Handlers;

public abstract class BaseWebhookHandler : BaseInvocable, IWebhookEventHandler, IAsyncRenewableWebhookEventHandler
{
    private string BridgeWebhooksUrl = ""; 
    
    private readonly string _subscriptionEvent;
    protected readonly IWebhookHandlerInput WebhookInput;

    protected BaseWebhookHandler(InvocationContext invocationContext, string subscriptionEvent) : base(invocationContext)
    {
        _subscriptionEvent = subscriptionEvent;
        BridgeWebhooksUrl = InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/') + $"/webhooks/{ApplicationConstants.AppName}";
    }

    protected BaseWebhookHandler(InvocationContext invocationContext, string subscriptionEvent, [WebhookParameter(true)] IWebhookHandlerInput input) : this(invocationContext, subscriptionEvent)
    {
        WebhookInput = input;
    }

    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var client = new MSTeamsClient(authenticationCredentialsProviders);
        var resource = GetResource();
        var subscription = await GetTargetSubscription(client);

        if (subscription is null)
        {
            subscription = await client.Subscriptions.PostAsync(new Subscription
            {
                ChangeType = _subscriptionEvent,
                NotificationUrl = BridgeWebhooksUrl,
                Resource = resource,
                ExpirationDateTime = DateTimeOffset.Now + TimeSpan.FromMinutes(60),
                ClientState = ApplicationConstants.ClientState
            });
        }

        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/'));
        await bridgeService.Subscribe(values["payloadUrl"], subscription.Id, _subscriptionEvent);
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var client = new MSTeamsClient(authenticationCredentialsProviders);
        var subscription = await GetTargetSubscription(client);
        
        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/'));
        var webhooksLeft = await bridgeService.Unsubscribe(values["payloadUrl"], subscription!.Id, _subscriptionEvent);

        if (webhooksLeft == 0)
            await client.Subscriptions[subscription.Id].DeleteAsync();
    }
    
    [Period(59)]
    public async Task RenewSubscription(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        var client = new MSTeamsClient(authenticationCredentialsProviders);
        var subscription = await GetTargetSubscription(client);

        var requestBody = new Subscription
        {
            ExpirationDateTime = DateTimeOffset.Now + TimeSpan.FromMinutes(60)
        };
        await client.Subscriptions[subscription!.Id].PatchAsync(requestBody);
    }

    private async Task<Subscription?> GetTargetSubscription(MSTeamsClient client)
    {
        var resource = GetResource();
        var subscription = (await client.Subscriptions.GetAsync()).Value
            .FirstOrDefault(s => s.NotificationUrl == BridgeWebhooksUrl && s.Resource == resource 
                                                                        && s.ChangeType == _subscriptionEvent);
        return subscription;
    }
    
    protected abstract string GetResource();
}