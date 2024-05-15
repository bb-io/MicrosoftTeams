using RestSharp;

namespace Apps.MicrosoftTeams;

public class Logger
{
    private readonly string _logUrl = "https://webhook.site/806f4b77-7442-4273-b772-3bc20d914367";
    
    public async Task Log<T>(T obj)
        where T : class
    {
        var client = new RestClient(_logUrl);
        await client.ExecuteAsync(new RestRequest(string.Empty, Method.Post)
            .AddJsonBody(obj));
    }
}