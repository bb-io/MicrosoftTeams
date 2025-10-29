using Newtonsoft.Json;
using Tests.MicrosoftTeams.Base;
using Apps.MicrosoftTeams.Actions;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Webhooks.Lists;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Apps.MicrosoftTeams.Webhooks.Inputs;
using Newtonsoft.Json.Linq;

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


    [TestMethod]
    public async Task WebhookTest_TopLevelMessage_PayloadOnly()
    {
        // Arrange
        var action = new ChannelWebhooks(InvocationContext);

        const string topLevelPayload = """
        {
          "SubscriptionId": "de9b3539-ce5a-4bda-a346-9a91a45f68fe",
          "ClientState": "4c93ca95-2d05-45ec-be28-6d49a213eed6",
          "ChangeType": "created",
          "Resource": "teams('33189cfd-6664-4e5e-84c6-545e02af51cd')/channels('19:60f2e072f7e745168db1b9bbdf4d3522@thread.tacv2')/messages('1761603520359')",
          "SubscriptionExpirationDateTime": "2025-10-30T16:57:08.4753052+00:00",
          "ResourceData": {
            "Id": "1761603520359",
            "@odata.type": "#Microsoft.Graph.chatMessage",
            "@odata.id": "teams('33189cfd-6664-4e5e-84c6-545e02af51cd')/channels('19:60f2e072f7e745168db1b9bbdf4d3522@thread.tacv2')/messages('1761603520359')"
          }
        }
        """;

        var payload = new WebhookRequest
        {
            Body = JToken.Parse(topLevelPayload)
        };

        var sender = new SenderInput
        {
        };

        // Act
        var result = await action.OnMessageWithAttachmentSent(payload, sender);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.HttpResponseMessage);
    }

    [TestMethod]
    public async Task WebhookTest_Reply_PayloadOnly()
    {
        // Arrange
        var action = new ChannelWebhooks(InvocationContext);

        const string replyPayload = """
    {
      "SubscriptionId": "de9b3539-ce5a-4bda-a346-9a91a45f68fe",
      "ClientState": "4c93ca95-2d05-45ec-be28-6d49a213eed6",
      "ChangeType": "created",
      "Resource": "teams('33189cfd-6664-4e5e-84c6-545e02af51cd')/channels('19:60f2e072f7e745168db1b9bbdf4d3522@thread.tacv2')/messages('1761603520359')/replies('1761603537476')",
      "SubscriptionExpirationDateTime": "2025-10-30T16:57:08.4753052+00:00",
      "ResourceData": {
        "Id": "1761603537476",
        "@odata.type": "#Microsoft.Graph.chatMessage",
        "@odata.id": "teams('33189cfd-6664-4e5e-84c6-545e02af51cd')/channels('19:60f2e072f7e745168db1b9bbdf4d3522@thread.tacv2')/messages('1761603520359')/replies('1761603537476')"
      }
    }
    """;

        var payload = new WebhookRequest
        {
            Body = JToken.Parse(replyPayload)
        };

        var sender = new SenderInput
        {
        };

        // Act
        var result = await action.OnMessageWithAttachmentSent(payload, sender);

        // Assert
        Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.HttpResponseMessage);
    }
}
