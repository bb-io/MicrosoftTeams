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
                ExpirationDateTime = DateTimeOffset.Now + TimeSpan.FromMinutes(4000),
                ClientState = ApplicationConstants.ClientState,
                LifecycleNotificationUrl = BridgeWebhooksUrl
            });

            InvocationContext.Logger?.LogInformation(
               "[MicrosoftTeamsHandleWebhookRequest] Subscribed: Event={Event}, Resource={Resource}, SubscriptionId={Id}, Expires={Expiry}, PayloadUrl={Url}",
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
                "[MicrosoftTeamsHandleWebhookRequest] Subscription already exists: Event={Event}, Resource={Resource}, SubscriptionId={Id}, PayloadUrl={Url}",
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
        var subscription = await GetTargetSubscription(client);

        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/'));
        var webhooksLeft = await bridgeService.Unsubscribe(values["payloadUrl"], subscription!.Id, _subscriptionEvent);

        InvocationContext.Logger?.LogInformation(
            "[MicrosoftTeamsHandleWebhookRequest] Unsubscribed: Event={Event}, Resource={Resource}, SubscriptionId={Id}, PayloadUrl={Url}, Remaining={Count}",
            new object[]
            {
                _subscriptionEvent,
                subscription.Resource,
                subscription.Id,
                values["payloadUrl"],
                webhooksLeft
            });

        if (webhooksLeft == 0)
        {
            InvocationContext.Logger?.LogInformation(
                "[MicrosoftTeamsHandleWebhookRequest] About to delete Graph subscription: Id={SubscriptionId}",
                new object[] { subscription.Id }
            );

            try
            {
                await client.Subscriptions[subscription.Id].DeleteAsync();

                InvocationContext.Logger?.LogInformation(
                    "[MicrosoftTeamsHandleWebhookRequest] Subscription deleted: Id={SubscriptionId}",
                    new object[] { subscription.Id }
                );
            }
            catch (Exception ex)
            {
                InvocationContext.Logger?.LogError(
                    "[MicrosoftTeamsHandleWebhookRequest] Failed to delete subscription: Id={SubscriptionId}",
                    new object[] { subscription.Id }
                );
                throw;
            }
        }
    }
        

    [Period(3950)]
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
                ExpirationDateTime = DateTimeOffset.Now + TimeSpan.FromMinutes(4000)
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