using Apps.MicrosoftTeams.Dtos;
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

namespace Apps.MicrosoftTeams.Actions;

[ActionList("Chats")]
public class ChatActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : MsTeamsInvocable(invocationContext)
{
    [Action("List chats", Description = "List chats")]
    public async Task<ListChatsResponse> ListChats()
    {
        var top = 50;
        var allChats = new List<Chat>();
        
        var chats = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client.Me.Chats.GetAsync(requestConfiguration => { requestConfiguration.QueryParameters.Top = top; }));
        
        var maxIterations = 10;
        while (chats?.Value != null && maxIterations > 0)
        {
            allChats.AddRange(chats.Value);

            if (chats.Value.Count == 0)
            {
                break;
            }

            if (!string.IsNullOrEmpty(chats.OdataNextLink))
            {
                chats = await Client.ExecuteWithErrorHandlingAsync(() => Client.Me.Chats.WithUrl(chats.OdataNextLink).GetAsync());
                maxIterations--;
            }
            else
            {
                break;
            }
        }
        
        return new ListChatsResponse
        {
            Chats = allChats.Select(chat => new ChatDto(chat)).ToList()
        };
    }

    [Action("Get chat message", Description = "Get chat message")]
    public async Task<ChatMessageDto> GetChatMessage([ActionParameter] ChatIdentifier chatIdentifier, 
        [ActionParameter] MessageIdentifier messageIdentifier)
    {
        var message = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client.Me
                .Chats[chatIdentifier.ChatId]
                .Messages[messageIdentifier.MessageId]
                .GetAsync());
        
        return new ChatMessageDto(message);
    }
        
    [Action("Download files attached to chat message", Description = "Download files attached to chat message")]
    public async Task<DownloadFilesAttachedToMessageResponse> DownloadFilesAttachedToMessage(
        [ActionParameter] ChatIdentifier chatIdentifier, 
        [ActionParameter] MessageIdentifier messageIdentifier)
    {
        var message = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client.Me
                .Chats[chatIdentifier.ChatId]
                .Messages[messageIdentifier.MessageId]
                .GetAsync());
        
        var fileAttachments = message?.Attachments?.Where(a => a.ContentType == "reference") ?? [];
        var resultFiles = new List<FileReference>();

        foreach (var attachment in fileAttachments)
        {
            var sharingUrl = attachment.ContentUrl;
            var base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sharingUrl));
            var encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/','_').Replace('+','-');
            var fileData = await Client.ExecuteWithErrorHandlingAsync(() => Client.Shares[encodedUrl].DriveItem.GetAsync());

            var fileContentStream = await Client.ExecuteWithErrorHandlingAsync(() =>
                Client.Shares[encodedUrl].DriveItem.Content.GetAsync());
            
            var memoryStream = new MemoryStream();
            await fileContentStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var file = await fileManagementClient.UploadAsync(memoryStream, fileData.File.MimeType, fileData.Name);
            resultFiles.Add(file);
        }
        
        return new DownloadFilesAttachedToMessageResponse { Files = resultFiles.Select(file => new FileDto(file)) };
    }

    [Action("Get the most recent chat messages", Description = "Get the most recent chat messages")]
    public async Task<GetLastMessages> GetLastMessages([ActionParameter] ChatIdentifier chatIdentifier,
        [ActionParameter] [Display("Messages amount")] int messagesAmount)
    {
        var messages = await Client.ExecuteWithErrorHandlingAsync(() => Client.Me.Chats[chatIdentifier.ChatId].Messages.GetAsync());
        return new GetLastMessages
        {
            Messages = messages.Value.Take(messagesAmount).Select(m => new ChatMessageDto(m))
        };
    }

    [Action("Send message to chat", Description = "Send message to chat")]
    public async Task<ChatMessageDto> SendMessageToChat([ActionParameter] ChatIdentifier chatIdentifier,
        [ActionParameter] SendMessageRequest input)
    {
        var requestBody = await CreateChatMessage(Client, input);

        var sentMessage = await Client.ExecuteWithErrorHandlingAsync(() => 
            Client.Me.Chats[chatIdentifier.ChatId].Messages.PostAsync(requestBody));
        
        return new ChatMessageDto(sentMessage);
    }

    [Action("Delete message from chat", Description = "Delete message from chat")]
    public async Task DeleteMessageFromChat([ActionParameter] ChatIdentifier chatIdentifier, 
        [ActionParameter] MessageIdentifier messageIdentifier)
    {
        await Client.ExecuteWithErrorHandlingAsync(() => 
            Client.Me
                .Chats[chatIdentifier.ChatId]
                .Messages[messageIdentifier.MessageId]
                .SoftDelete
                .PostAsync());
    }
        
    private async Task<ChatMessage> CreateChatMessage(MSTeamsClient Client, SendMessageRequest input)
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

        if (input.AttachmentFile is null && input.OneDriveAttachmentFileId is null) 
            return requestBody;
        
        var drive = await Client.ExecuteWithErrorHandlingAsync(() => Client.Me.Drive.GetAsync());

        if (input.OneDriveAttachmentFileId is not null)
        {
            var oneDriveAttachmentFile = await Client.ExecuteWithErrorHandlingAsync(() => 
                Client
                    .Drives[drive.Id]
                    .Items[input.OneDriveAttachmentFileId]
                    .GetAsync());
            
            var attachmentId = oneDriveAttachmentFile.ETag.Split("{")[1].Split("}")[0];
            requestBody.Attachments.Add(new()
            {
                Id = attachmentId,
                ContentType = "reference",
                ContentUrl = oneDriveAttachmentFile.WebUrl,
                Name = oneDriveAttachmentFile.Name
            });
            requestBody.Body.Content += $"<attachment id=\"{attachmentId}\"></attachment>";
        }

        if (input.AttachmentFile is not null)
        {
            var attachmentFile = await UploadFile(input.AttachmentFile);
            var attachmentId = attachmentFile.ETag.Split("{")[1].Split("}")[0];
            var webUrl = Path.GetExtension(attachmentFile.Name) == ".docx"
                ? attachmentFile.WebUrl.Split("&action")[0]
                : attachmentFile.WebUrl; 
                    
            requestBody.Attachments.Add(new()
            {
                Id = attachmentId,
                ContentType = "reference",
                ContentUrl = webUrl,
                Name = attachmentFile.Name
            });
            requestBody.Body.Content += $"<attachment id=\"{attachmentId}\"></attachment>";
        }

        return requestBody;
    }
        
    private async Task<DriveItem> UploadFile(FileReference file)
    {
        const string teamsFilesFolderName = "Microsoft Teams Chat Files";
        const int chunkSize = 3932160; 
        
        var drive = await Client.Me.Drive.GetAsync();
        var root = await Client.Drives[drive.Id].Root.GetAsync();
        var folders = await Client.Drives[drive.Id].Items[root.Id].Children.GetAsync();
        var teamsFilesFolder = folders.Value.FirstOrDefault(folder =>
            folder.Folder is not null && folder.Name == teamsFilesFolderName);

        if (teamsFilesFolder is null)
            teamsFilesFolder = await Client.Drives[drive.Id].Items[root.Id].Children.PostAsync(new DriveItem
            {
                Name = teamsFilesFolderName,
                Folder = new Folder()
            });
        
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
            
        var uploadSession = await Client.Drives[drive.Id].Items[teamsFilesFolder.Id].ItemWithPath(file.Name)
            .CreateUploadSession.PostAsync(uploadSessionRequestBody);

        var fileUploadTask =
            new LargeFileUploadTask<DriveItem>(uploadSession, fileMemoryStream, chunkSize, Client.RequestAdapter);
        var uploadResult = await fileUploadTask.UploadAsync();
        return uploadResult.ItemResponse;
    }
}
