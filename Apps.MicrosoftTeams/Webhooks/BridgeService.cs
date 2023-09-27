using Apps.MicrosoftTeams.Webhooks.Payload;
using RestSharp;

namespace Apps.MicrosoftTeams.Webhooks;

public class BridgeService
{
    private const string AppName = ApplicationConstants.AppName;
    
    private readonly RestClient _bridgeClient;

    public BridgeService()
    {
        _bridgeClient = new RestClient(new RestClientOptions(ApplicationConstants.BridgeServiceUrl));
    }

    public async Task Subscribe(string url, string id, string subscriptionEvent)
    {
        var bridgeSubscriptionRequest = CreateBridgeRequest($"/webhooks/{AppName}/{id}/{subscriptionEvent}", Method.Post);
        bridgeSubscriptionRequest.AddBody(url);
        await _bridgeClient.ExecuteAsync(bridgeSubscriptionRequest);
    }
    
    public async Task<int> Unsubscribe(string url, string id, string subscriptionEvent)
    {
        var getTriggerRequest = CreateBridgeRequest($"/webhooks/{AppName}/{id}/{subscriptionEvent}", Method.Get);
        var webhooks = await _bridgeClient.GetAsync<List<BridgeGetResponse>>(getTriggerRequest);
        var webhook = webhooks.FirstOrDefault(w => w.Value == url);

        var deleteTriggerRequest = CreateBridgeRequest($"/webhooks/{AppName}/{id}/{subscriptionEvent}/{webhook.Id}", 
            Method.Delete);
        await _bridgeClient.ExecuteAsync(deleteTriggerRequest);

        var webhooksLeft = webhooks.Count - 1;
        return webhooksLeft;
    }
    
    private RestRequest CreateBridgeRequest(string endpoint, Method method)
    {
        var request = new RestRequest(endpoint, method);
        request.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        return request;
    }
}