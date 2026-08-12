using Apps.MicrosoftTeams.Models.Utility;
using Microsoft.Graph.Models;

namespace Tests.MicrosoftTeams;

[TestClass]
public class ChatMessageMentionBuilderTests
{
    [TestMethod]
    public void Apply_PlainText_DoesNotSetMentionsToNull()
    {
        var message = new ChatMessage
        {
            Body = new ItemBody { ContentType = BodyType.Html }
        };

        ChatMessageMentionBuilder.Apply(message, "Hello team");

        Assert.AreEqual("Hello team", message.Body.Content);
        Assert.IsNull(message.Mentions);
        CollectionAssert.DoesNotContain(
            message.BackingStore.EnumerateKeysForValuesChangedToNull().ToList(),
            "mentions");
    }
}
