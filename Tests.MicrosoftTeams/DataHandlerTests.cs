using Blackbird.Applications.Sdk.Common.Webhooks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Tests.MicrosoftTeams.Base;
using Apps.MicrosoftTeams.Webhooks.Lists;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Azure;

namespace Tests.MicrosoftTeams;

[TestClass]
public class DataHandlerTests : TestBase
{
    [TestMethod]
    public async Task ChannelDataHandler_IsSuccess()
    {
        var handler = new ChannelHandler(InvocationContext);

        var response = await handler.GetDataAsync(new DataSourceContext { SearchString=""}, CancellationToken.None);
    
        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);
    }

    [TestMethod]
    public async Task ChatDataHandler_IsSuccess()
    {
        var handler = new ChatHandler(InvocationContext);

        var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

        var json = JsonConvert.SerializeObject(response, Formatting.Indented);
        Console.WriteLine(json);
    }

    ///
}
