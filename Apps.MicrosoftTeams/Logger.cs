using RestSharp;

namespace Apps.MicrosoftTeams;

public class Logger
{
    private readonly string _logUrl = "https://webhook.site/45674b9f-0059-47a9-b5cd-542aef154dff";
    
    public async Task Log<T>(T obj)
        where T : class
    {
        var client = new RestClient(_logUrl);
        await client.ExecuteAsync(new RestRequest(string.Empty, Method.Post)
            .AddJsonBody(obj));
    }
}