using Newtonsoft.Json;
using Tests.MicrosoftTeams.Base;
using Apps.MicrosoftTeams.Actions;
using Apps.MicrosoftTeams.Models.Identifiers;

namespace Tests.MicrosoftTeams;

[TestClass]
public class ChatTests : TestBase
{
    [TestMethod]
    public async Task DownloadFilesAttachedToMessage_IsSuccess()
    {
		// Arrange
		var action = new ChatActions(InvocationContext, FileManager);
        var chat = new ChatIdentifier { ChatId = "19:2dfdb508-a146-49f9-85ee-985dacc1ab9c_440d4d44-b356-40c3-a819-936370f9a0b9@unq.gbl.spaces" };
        var message = new MessageIdentifier { MessageId = "1764950235452" };

        // Act
        var result = await action.DownloadFilesAttachedToMessage(chat, message);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
    }
}
