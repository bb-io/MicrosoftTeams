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

        InvocationContext.Logger?.LogInformation(
           $"[MicrosoftTeamsHandleWebhookRequest] Subscription method started; Bird info: {InvocationContext.Bird?.Id}" +
           $"Flight info: {InvocationContext.Flight?.Id}, Tenant info:{InvocationContext.Tenant?.Id}", []);

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

            InvocationContext.Logger?.LogInformation(
               "[TeamsWebhook] Subscribed: Event={Event}, Resource={Resource}, SubscriptionId={Id}, Expires={Expiry}, PayloadUrl={Url}",
               new object[]
               {
                    _subscriptionEvent,
                    resource,
                    subscription?.Id,
                    subscription?.ExpirationDateTime,
                    values["payloadUrl"]
               });
        }
        else
        {
            InvocationContext.Logger?.LogInformation(
                "[TeamsWebhook] Subscription already exists: Event={Event}, Resource={Resource}, SubscriptionId={Id}, PayloadUrl={Url}",
                new object[]
                {
                    _subscriptionEvent,
                    resource,
                    subscription?.Id,
                    values["payloadUrl"]
                });
        }

        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/'));
        await bridgeService.Subscribe(values["payloadUrl"], subscription.Id, _subscriptionEvent);
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        InvocationContext.Logger?.LogInformation(
           $"[MicrosoftTeamsHandleWebhookRequest] Unsubscription method started; Bird info: {InvocationContext.Bird?.Id}" +
           $"Flight info: {InvocationContext.Flight?.Id}, Tenant info:{InvocationContext.Tenant?.Id}", []);

        var client = new MSTeamsClient(authenticationCredentialsProviders);
        Subscription? subscription = null;
        try
        {
            subscription = await GetTargetSubscription(client);
            InvocationContext.Logger?.LogInformation(
                "[TeamsWebhook] After GetTargetSubscription: SubscriptionId={Id}, Resource={Resource}",
                 new object[]
                {
                subscription?.Id ?? "null",
                subscription?.Resource ?? "null"
                }
            );
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"[MicrosoftTeamsHandleWebhookRequest] Error in GetTargetSubscription {ex.Message}- {ex.InnerException}", []
            );
            throw;
        }

        if (subscription == null)
        {
            InvocationContext.Logger?.LogWarning(
                "[MicrosoftTeamsHandleWebhookRequest] No existing subscription found for resource {Resource}", []
            );
            return;
        }

        try
        {
            var payloadUrl = values.ContainsKey("payloadUrl") ? values["payloadUrl"] : "missing";
            InvocationContext.Logger?.LogInformation(
                "[MicrosoftTeamsHandleWebhookRequest] Calling bridgeService.Unsubscribe: SubscriptionId={Id}, PayloadUrl={Url}",
                 new object[]
                {
                subscription.Id,
                payloadUrl
                }
            );

            var bridgeService = new BridgeService(
                InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')
            );
            var webhooksLeft = await bridgeService.Unsubscribe(
                payloadUrl,
                subscription.Id,
                _subscriptionEvent
            );

            InvocationContext.Logger?.LogInformation(
                "[MicrosoftTeamsHandleWebhookRequest] Unsubscribed: Event={Event}, Resource={Resource}, SubscriptionId={Id}, PayloadUrl={Url}, Remaining={Count}",
                 new object[]
                {
                _subscriptionEvent,
                subscription.Resource,
                subscription.Id,
                payloadUrl,
                webhooksLeft
                }
            );

            if (webhooksLeft == 0)
            {
                await client.Subscriptions[subscription.Id].DeleteAsync();
                InvocationContext.Logger?.LogInformation(
                    $"[TeamsWebhook] Subscription deleted: Id={subscription.Id}", []);
            }
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"[TeamsWebhook] Exception during UnsubscribeAsync {ex.Message} - {ex.InnerException}",
                []
            );
            throw;
        }
    }

    [Period(50)]
    public async Task RenewSubscription(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        Dictionary<string, string> values)
    {
        InvocationContext.Logger?.LogInformation(
           $"[MicrosoftTeamsHandleWebhookRequest] RenewSubscription method started; Bird info: {InvocationContext.Bird?.Id}" +
           $"Flight info: {InvocationContext.Flight?.Id}, Tenant info:{InvocationContext.Tenant?.Id}", []);

        var client = new MSTeamsClient(authenticationCredentialsProviders);

        try
        {
            var subscription = await GetTargetSubscription(client);

            var requestBody = new Subscription
            {
                ExpirationDateTime = DateTimeOffset.Now + TimeSpan.FromMinutes(60)
            };
            var updatedSubscription = await client.Subscriptions[subscription!.Id].PatchAsync(requestBody);

            InvocationContext.Logger?.LogInformation(
                $"[MicrosoftTeamsHandleWebhookRequest] Successfully renewed subscription {subscription.Id}. " +
                $"Response - Id: {updatedSubscription?.Id}, ExpirationDateTime: {updatedSubscription?.ExpirationDateTime}, " +
                $"Resource: {updatedSubscription?.Resource}, NotificationUrl: {updatedSubscription?.NotificationUrl}", []);
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogInformation(
                $"[MicrosoftTeamsHandleWebhookRequest] RenewSubscription method failed with error: {ex.Message}; StackTrace: {ex.StackTrace}; Bird info: {InvocationContext.Bird?.Id}" +
                $"Flight info: {InvocationContext.Flight?.Id}, Tenant info:{InvocationContext.Tenant?.Id}", []);
            throw;
        }
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