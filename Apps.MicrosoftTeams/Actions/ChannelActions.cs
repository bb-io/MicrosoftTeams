using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.DynamicHandlers;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Models.Requests;
using Apps.MicrosoftTeams.Models.Responses;
using Apps.MicrosoftTeams.Models.Utility;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.MicrosoftTeams.Actions;

[ActionList("Channels")]
public class ChannelActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : MsTeamsInvocable(invocationContext)
{
    [Action("Get channel message", Description = "Get channel message")]
    public async Task<ChannelMessageDto> GetChannelMessage([ActionParameter] ChannelIdentifier channelIdentifier,
        [ActionParameter] MessageIdentifier messageIdentifier)
    {
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId);

        var message = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client
                .Teams[teamChannel.TeamId]
                .Channels[teamChannel.ChannelId]
                .Messages[messageIdentifier.MessageId]
                .GetAsync());
        
        return new ChannelMessageDto(message);
    }

    [Action("Download files attached to channel message", Description = "Download files attached to channel message")]
    public async Task<DownloadFilesAttachedToMessageResponse> DownloadFilesAttachedToMessage(
        [ActionParameter] ChannelIdentifier channelIdentifier,
        [ActionParameter] ChannelMessageIdentifier messageIdentifier)
    {
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId)
            ?? throw new PluginApplicationException("Could not resolve the selected channel.");
        var rootMessage = Client
            .Teams[teamChannel.TeamId]
            .Channels[teamChannel.ChannelId]
            .Messages[messageIdentifier.MessageId];

        ChatMessage? message;
        Func<string, Task<Stream?>> getHostedContent;

        if (!string.IsNullOrWhiteSpace(messageIdentifier.ReplyId))
        {
            var reply = rootMessage.Replies[messageIdentifier.ReplyId];
            message = await Client.ExecuteWithErrorHandlingAsync(() => reply.GetAsync());
            getHostedContent = hostedContentId => Client.ExecuteWithErrorHandlingAsync(() =>
                reply.HostedContents[hostedContentId].Content.GetAsync());
        }
        else
        {
            message = await Client.ExecuteWithErrorHandlingAsync(() => rootMessage.GetAsync());
            getHostedContent = hostedContentId => Client.ExecuteWithErrorHandlingAsync(() =>
                rootMessage.HostedContents[hostedContentId].Content.GetAsync());
        }

        if (message is null)
            throw new PluginApplicationException("Microsoft Graph did not return the requested channel message.");
        
        var fileAttachments = message.Attachments?
            .Where(a => a.ContentType == "reference")
            ?? Enumerable.Empty<ChatMessageAttachment>();
        var resultFiles = new List<FileReference>();

        foreach (var attachment in fileAttachments)
        {
            var sharingUrl = attachment.ContentUrl;
            if (string.IsNullOrWhiteSpace(sharingUrl))
                throw new PluginApplicationException("The attached file does not contain a download URL.");

            var base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sharingUrl));
            var encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');
            var fileData = await Client.ExecuteWithErrorHandlingAsync(async () => await Client.Shares[encodedUrl].DriveItem.GetAsync());

            var fileContentStream = await Client.ExecuteWithErrorHandlingAsync(async () => 
                await Client.Shares[encodedUrl].DriveItem.Content.GetAsync());

            if (fileContentStream is null || fileData?.File is null || string.IsNullOrWhiteSpace(fileData.Name))
                throw new PluginApplicationException("Microsoft Graph did not return the attached file content or metadata.");
            
            var memoryStream = new MemoryStream();
            await fileContentStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var mimeType = fileData.File.MimeType ?? "application/octet-stream";
            var file = await fileManagementClient.UploadAsync(memoryStream, mimeType, fileData.Name);
            resultFiles.Add(file);
        }

        var hostedContentIds = HostedContentImageHelper.GetIds(message.Body?.Content);
        for (var index = 0; index < hostedContentIds.Count; index++)
        {
            var hostedContentStream = await getHostedContent(hostedContentIds[index]);
            if (hostedContentStream is null)
                throw new PluginApplicationException("Microsoft Graph did not return the inline image content.");

            var memoryStream = new MemoryStream();
            await hostedContentStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var (contentType, extension) = HostedContentImageHelper.DetectImageType(memoryStream);
            var fileName = $"inline-image-{index + 1}{extension}";
            var file = await fileManagementClient.UploadAsync(memoryStream, contentType, fileName);
            resultFiles.Add(file);
        }

        return new DownloadFilesAttachedToMessageResponse
        {
            Files = resultFiles.Select(file => new FileDto(file))
        };
    }

    [Action("Send message to channel", Description = "Send message to channel")]
    public async Task<ChannelMessageDto> SendMessageToChannel(
        [ActionParameter] ChannelIdentifier channelIdentifier,
        [ActionParameter] SendMessageRequest input)
    {
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId);
        var requestBody = await CreateChannelMessage(input, teamChannel);

        var sentMessage = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client
                .Teams[teamChannel.TeamId]
                .Channels[teamChannel.ChannelId]
                .Messages
                .PostAsync(requestBody));
        
        return new ChannelMessageDto(sentMessage);
    }

    [Action("Reply to message in channel", Description = "Reply to message in channel")]
    public async Task<ChannelMessageDto> ReplyToMessageInChannel(
        [ActionParameter] ChannelIdentifier channelIdentifier,
        [ActionParameter] MessageIdentifier messageIdentifier,
        [ActionParameter] SendMessageRequest input)
    {
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId);
        var requestBody = await CreateChannelMessage(input, teamChannel);

        var sentReply = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client
                .Teams[teamChannel.TeamId]
                .Channels[teamChannel.ChannelId]
                .Messages[messageIdentifier.MessageId]
                .Replies
                .PostAsync(requestBody));
        
        return new ChannelMessageDto(sentReply);
    }

    private async Task<ChatMessage> CreateChannelMessage(
        SendMessageRequest input,
        TeamChannel teamChannel)
    {
        var requestBody = new ChatMessage
        {
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = input.Message
            },
            Attachments = new List<ChatMessageAttachment>()
        };

        await ChatMessageMentionBuilder.ApplyAsync(Client, requestBody, input);

        if (input.AttachmentFile is not null || input.OneDriveAttachmentFileId is not null)
        {
            if (input.OneDriveAttachmentFileId is not null)
                throw new PluginApplicationException("OneDrive attachments are not supported for channel messages. Please use Attachment file instead.");

            if (input.AttachmentFile is not null)
            {
                var attachmentFile = await UploadFile(input.AttachmentFile, teamChannel);
                var attachmentId = attachmentFile.ETag.Split("{")[1].Split("}")[0];
                var webUrl = Path.GetExtension(attachmentFile.Name).Equals(".docx", StringComparison.OrdinalIgnoreCase)
                    ? attachmentFile.WebUrl.Split("&action")[0]
                    : attachmentFile.WebUrl;

                requestBody.Attachments.Add(new ChatMessageAttachment
                {
                    Id = attachmentId,
                    ContentType = "reference",
                    ContentUrl = webUrl,
                    Name = attachmentFile.Name
                });

                requestBody.Body.Content += $"<attachment id=\"{attachmentId}\"></attachment>";
            }
        }

        return requestBody;
    }

    private async Task<DriveItem> UploadFile(FileReference file, TeamChannel teamChannel)
    {
        const int chunkSize = 3932160;

        var channelFolder = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client
                .Teams[teamChannel.TeamId]
                .Channels[teamChannel.ChannelId]
                .FilesFolder
                .GetAsync());

        if (channelFolder is null)
            throw new PluginApplicationException("Could not resolve the channel files folder.");

        if (string.IsNullOrEmpty(channelFolder.Id))
            throw new PluginApplicationException("Could not resolve the channel folder ID.");

        if (string.IsNullOrEmpty(channelFolder.ParentReference?.DriveId))
            throw new PluginApplicationException("Could not resolve the channel SharePoint drive ID.");

        var fileStream = await fileManagementClient.DownloadAsync(file);
        var fileMemoryStream = new MemoryStream();
        await fileStream.CopyToAsync(fileMemoryStream);
        fileMemoryStream.Position = 0;

        var uploadSessionRequestBody = new CreateUploadSessionPostRequestBody
        {
            Item = new DriveItemUploadableProperties
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "@microsoft.graph.conflictBehavior", "rename" }
                }
            }
        };

        var uploadSession = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client
                .Drives[channelFolder.ParentReference.DriveId]
                .Items[channelFolder.Id]
                .ItemWithPath(file.Name)
                .CreateUploadSession
                .PostAsync(uploadSessionRequestBody));

        var fileUploadTask =
            new LargeFileUploadTask<DriveItem>(uploadSession, fileMemoryStream, chunkSize, Client.RequestAdapter);

        var uploadResult = await fileUploadTask.UploadAsync();

        if (!uploadResult.UploadSucceeded || uploadResult.ItemResponse is null)
            throw new PluginApplicationException("Failed to upload the file to the channel SharePoint folder.");

        return uploadResult.ItemResponse;
    }
}
