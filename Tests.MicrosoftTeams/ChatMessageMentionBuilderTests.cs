using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Utility;
using Microsoft.Graph.Models;

namespace Tests.MicrosoftTeams;

[TestClass]
public class ChatMessageMentionBuilderTests
{
    [TestMethod]
    public void Apply_InlineMention_PreservesPositionAndCreatesGraphMention()
    {
        var message = CreateMessage();

        ChatMessageMentionBuilder.Apply(
            message,
            "Hello <at user_id=\"user-1\">John Doe</at>, please review this task.");

        Assert.AreEqual("Hello <at id=\"0\">John Doe</at>, please review this task.", message.Body?.Content);
        Assert.IsNotNull(message.Mentions);
        Assert.AreEqual(1, message.Mentions.Count);
        Assert.AreEqual(0, message.Mentions[0].Id);
        Assert.AreEqual("John Doe", message.Mentions[0].MentionText);
        Assert.AreEqual("user-1", message.Mentions[0].Mentioned?.User?.Id);
        Assert.AreEqual("aadUser", message.Mentions[0].Mentioned?.User?.AdditionalData["userIdentityType"]);
    }

    [TestMethod]
    public void Apply_AppendedUsers_AddsUniqueMentionsToEnd()
    {
        var message = CreateMessage();
        var users = new[]
        {
            new User { Id = "user-1", DisplayName = "John Doe" },
            new User { Id = "user-1", DisplayName = "John Doe" },
            new User { Id = "user-2", DisplayName = "Anna Smith" }
        };

        ChatMessageMentionBuilder.Apply(message, "Please review this task.", users);

        Assert.AreEqual(
            "Please review this task. <at id=\"0\">John Doe</at> <at id=\"1\">Anna Smith</at>",
            message.Body?.Content);
        Assert.IsNotNull(message.Mentions);
        Assert.AreEqual(2, message.Mentions.Count);
        Assert.AreEqual("user-1", message.Mentions[0].Mentioned?.User?.Id);
        Assert.AreEqual("user-2", message.Mentions[1].Mentioned?.User?.Id);
    }

    [TestMethod]
    public void Apply_NumericInlineMention_MapsToSelectedUserAndDoesNotAppendIt()
    {
        var message = CreateMessage();
        var users = new[]
        {
            new User { Id = "user-1", DisplayName = "John Doe" },
            new User { Id = "user-2", DisplayName = "Anna Smith" }
        };

        ChatMessageMentionBuilder.Apply(
            message,
            "Hello <at id=\"0\">John</at>, please review.",
            users);

        Assert.AreEqual(
            "Hello <at id=\"0\">John Doe</at>, please review. <at id=\"1\">Anna Smith</at>",
            message.Body?.Content);
        Assert.IsNotNull(message.Mentions);
        Assert.AreEqual(2, message.Mentions.Count);
        Assert.AreEqual("user-1", message.Mentions[0].Mentioned?.User?.Id);
        Assert.AreEqual("user-2", message.Mentions[1].Mentioned?.User?.Id);
    }

    [TestMethod]
    public void Apply_InlineAndAppendedMention_DoesNotAppendSameUserTwice()
    {
        var message = CreateMessage();
        var users = new[]
        {
            new User { Id = "user-1", DisplayName = "John Doe" },
            new User { Id = "user-2", DisplayName = "Anna Smith" }
        };

        ChatMessageMentionBuilder.Apply(
            message,
            "Hello <at user_id=\"user-1\">John Doe</at>",
            users);

        Assert.AreEqual(
            "Hello <at id=\"0\">John Doe</at> <at id=\"1\">Anna Smith</at>",
            message.Body?.Content);
        Assert.IsNotNull(message.Mentions);
        Assert.AreEqual(2, message.Mentions.Count);
    }

    [TestMethod]
    public void Apply_NoMentions_LeavesMessageUnchanged()
    {
        var message = CreateMessage();

        ChatMessageMentionBuilder.Apply(message, "Hello team");

        Assert.AreEqual("Hello team", message.Body?.Content);
        Assert.IsNull(message.Mentions);
    }

    [TestMethod]
    public void UserDto_MentionUser_ReturnsEncodedInlineToken()
    {
        var user = new UserDto(new User
        {
            Id = "user-1",
            DisplayName = "John & Jane"
        });

        Assert.AreEqual(
            "<at user_id=\"user-1\">John &amp; Jane</at>",
            user.MentionUser);
    }

    private static ChatMessage CreateMessage()
    {
        return new ChatMessage
        {
            Body = new ItemBody
            {
                ContentType = BodyType.Html
            }
        };
    }
}
