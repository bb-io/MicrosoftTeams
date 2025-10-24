using Newtonsoft.Json;
using Tests.MicrosoftTeams.Base;
using Apps.MicrosoftTeams.Actions;
using Apps.MicrosoftTeams.Models.Identifiers;

namespace Tests.MicrosoftTeams;

[TestClass]
public class ChannelTests : TestBase
{
    [TestMethod]
    public async Task DownloadFilesAttachedToMessage_ValidData_IsSuccess()
    {
		// Arrange
		var action = new ChannelActions(InvocationContext, FileManager);
        var channel = new ChannelIdentifier { TeamChannelId = "{\"TeamId\":\"33189cfd-6664-4e5e-84c6-545e02af51cd\",\"ChannelId\":\"19:60f2e072f7e745168db1b9bbdf4d3522@thread.tacv2\"}" };
        var message = new MessageIdentifier { MessageId = "1761073408631" };

        // Act
        var result = await action.DownloadFilesAttachedToMessage(channel, message);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }
}
