using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.DynamicHandlers;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Models.Requests;
using Apps.MicrosoftTeams.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.Content;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Newtonsoft.Json;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.MicrosoftTeams.Actions;

[ActionList]
public class ChannelActions : BaseInvocable
{
    private readonly IEnumerable<AuthenticationCredentialsProvider> _authenticationCredentialsProviders;

    public ChannelActions(InvocationContext invocationContext) : base(invocationContext)
    {
        _authenticationCredentialsProviders = invocationContext.AuthenticationCredentialsProviders;
    }

    [Action("Get channel message", Description = "Get channel message")]
    public async Task<ChannelMessageDto> GetChannelMessage([ActionParameter] ChannelIdentifier channelIdentifier, 
        [ActionParameter] MessageIdentifier messageIdentifier)
    {
        var client = new MSTeamsClient(_authenticationCredentialsProviders);
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId);

        try
        {
            var message = await client.Teams[teamChannel.TeamId].Channels[teamChannel.ChannelId]
                .Messages[messageIdentifier.MessageId].GetAsync();
            return new ChannelMessageDto(message);
        }
        catch (ODataError error)
        {
            throw new Exception(error.Error.Message);
        }
    }
    
    [Action("Download files attached to channel message", Description = "Download files attached to channel message")]
    public async Task<DownloadFilesAttachedToMessageResponse> DownloadFilesAttachedToMessage(
        [ActionParameter] ChannelIdentifier channelIdentifier, 
        [ActionParameter] MessageIdentifier messageIdentifier)
    {
        var client = new MSTeamsClient(_authenticationCredentialsProviders);
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId);

        try
        {
            var message = await client.Teams[teamChannel.TeamId].Channels[teamChannel.ChannelId]
                .Messages[messageIdentifier.MessageId].GetAsync();
            var fileAttachments = message.Attachments.Where(a => a.ContentType == "reference");
            var resultFiles = new List<File>();

            foreach (var attachment in fileAttachments)
            {
                var sharingUrl = attachment.ContentUrl;
                var base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sharingUrl));
                var encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/','_').Replace('+','-');
                var fileData = await client.Shares[encodedUrl].DriveItem.GetAsync();
                var fileContent = await client.Shares[encodedUrl].DriveItem.Content.GetAsync();
                var contentBytes = await fileContent.GetByteData();
                
                resultFiles.Add(new File(contentBytes)
                {
                    Name = fileData.Name,
                    ContentType = fileData.FileObject.MimeType
                });
            }
            
            return new DownloadFilesAttachedToMessageResponse { Files = resultFiles.Select(file => new FileDto(file)) };
        }
        catch (ODataError error)
        {
            throw new Exception(error.Error.Message);
        }
    }
    
    [Action("Send message to channel", Description = "Send message to channel")]
    public async Task<ChannelMessageDto> SendMessageToChannel([ActionParameter] ChannelIdentifier channelIdentifier, 
        [ActionParameter] SendMessageRequest input)
    {
        var client = new MSTeamsClient(_authenticationCredentialsProviders);
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId);
        var requestBody = await CreateChannelMessage(client, input);

        try
        {
            var sentMessage = await client.Teams[teamChannel.TeamId].Channels[teamChannel.ChannelId].Messages
                .PostAsync(requestBody);
            return new ChannelMessageDto(sentMessage);
        }
        catch (ODataError error)
        {
            throw new Exception(error.Error.Message);
        }
    }
    
    [Action("Reply to message in channel", Description = "Reply to message in channel")]
    public async Task<ChannelMessageDto> ReplyToMessageInChannel([ActionParameter] ChannelIdentifier channelIdentifier, 
        [ActionParameter] MessageIdentifier messageIdentifier, [ActionParameter] SendMessageRequest input)
    {
        var client = new MSTeamsClient(_authenticationCredentialsProviders);
        var teamChannel = JsonConvert.DeserializeObject<TeamChannel>(channelIdentifier.TeamChannelId);
        var requestBody = await CreateChannelMessage(client, input);

        try
        {
            var sentReply = await client.Teams[teamChannel.TeamId].Channels[teamChannel.ChannelId]
                .Messages[messageIdentifier.MessageId].Replies.PostAsync(requestBody);
            return new ChannelMessageDto(sentReply);
        }
        catch (ODataError error)
        {
            throw new Exception(error.Error.Message);
        }
    }

    private async Task<ChatMessage> CreateChannelMessage(MSTeamsClient client, SendMessageRequest input)
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

        try
        {
            if (input.AttachmentFile is not null || input.OneDriveAttachmentFileId is not null)
            {
                var drive = await client.Me.Drive.GetAsync();

                if (input.OneDriveAttachmentFileId is not null)
                {
                    var oneDriveAttachmentFile = await client.Drives[drive.Id].Items[input.OneDriveAttachmentFileId].GetAsync();
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
            }

            return requestBody;
        }
        catch (ODataError error)
        {
            throw new Exception(error.Error.Message);
        }
    } 
    
    private async Task<DriveItem> UploadFile(File file)
    {
        const string teamsFilesFolderName = "Microsoft Teams Chat Files";
        const int chunkSize = 3932160; 
        
        var client = new MSTeamsClient(InvocationContext.AuthenticationCredentialsProviders);
        var drive = await client.Me.Drive.GetAsync();
        var root = await client.Drives[drive.Id].Root.GetAsync();
        var folders = await client.Drives[drive.Id].Items[root.Id].Children.GetAsync();
        var teamsFilesFolder = folders.Value.FirstOrDefault(folder =>
            folder.Folder is not null && folder.Name == teamsFilesFolderName);

        if (teamsFilesFolder is null)
            teamsFilesFolder = await client.Drives[drive.Id].Items[root.Id].Children.PostAsync(new DriveItem
            {
                Name = teamsFilesFolderName,
                Folder = new Folder()
            });
        
        using var stream = new MemoryStream(file.Bytes);
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
            
        var uploadSession = await client.Drives[drive.Id].Items[teamsFilesFolder.Id].ItemWithPath(file.Name)
            .CreateUploadSession.PostAsync(uploadSessionRequestBody);

        var fileUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, stream, chunkSize, client.RequestAdapter);
        var uploadResult = await fileUploadTask.UploadAsync();
        return uploadResult.ItemResponse;
    }
}