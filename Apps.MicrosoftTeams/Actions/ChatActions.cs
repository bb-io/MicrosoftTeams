using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Models.Requests;
using Apps.MicrosoftTeams.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Apps.MicrosoftTeams.Actions
{
    [ActionList]
    public class ChatActions : BaseInvocable
    {
        private readonly IEnumerable<AuthenticationCredentialsProvider> _authenticationCredentialsProviders;
        private readonly IFileManagementClient _fileManagementClient;

        public ChatActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
            : base(invocationContext)
        {
            _authenticationCredentialsProviders = invocationContext.AuthenticationCredentialsProviders;
            _fileManagementClient = fileManagementClient;
        }
        
        [Action("List chats", Description = "List chats")]
        public async Task<ListChatsResponse> ListChats()
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);

            try
            {
                var chats = await client.Me.Chats.GetAsync();
                return new ListChatsResponse
                {
                    Chats = chats.Value.Select(chat => new ChatDto(chat)).ToList()
                };
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            }
        }

        [Action("Get chat message", Description = "Get chat message")]
        public async Task<ChatMessageDto> GetChatMessage([ActionParameter] ChatIdentifier chatIdentifier, 
            [ActionParameter] MessageIdentifier messageIdentifier)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);

            try
            {
                var message = await client.Me.Chats[chatIdentifier.ChatId].Messages[messageIdentifier.MessageId].GetAsync();
                return new ChatMessageDto(message);
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            }
        }
        
        [Action("Download files attached to chat message", Description = "Download files attached to chat message")]
        public async Task<DownloadFilesAttachedToMessageResponse> DownloadFilesAttachedToMessage(
            [ActionParameter] ChatIdentifier chatIdentifier, 
            [ActionParameter] MessageIdentifier messageIdentifier)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);

            try
            {
                var message = await client.Me.Chats[chatIdentifier.ChatId].Messages[messageIdentifier.MessageId].GetAsync();
                var fileAttachments = message.Attachments.Where(a => a.ContentType == "reference");
                var resultFiles = new List<FileReference>();

                foreach (var attachment in fileAttachments)
                {
                    var sharingUrl = attachment.ContentUrl;
                    var base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sharingUrl));
                    var encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/','_').Replace('+','-');
                    var fileData = await client.Shares[encodedUrl].DriveItem.GetAsync();
                    var fileContentStream = await client.Shares[encodedUrl].DriveItem.Content.GetAsync();
                    var file = await _fileManagementClient.UploadAsync(fileContentStream, fileData.FileObject.MimeType,
                        fileData.Name);
                    resultFiles.Add(file);
                }
            
                return new DownloadFilesAttachedToMessageResponse { Files = resultFiles.Select(file => new FileDto(file)) };
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            }
        }

        [Action("Get the most recent chat messages", Description = "Get the most recent chat messages")]
        public async Task<GetLastMessages> GetLastMessages([ActionParameter] ChatIdentifier chatIdentifier,
            [ActionParameter] [Display("Messages amount")] int messagesAmount)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);

            try
            {
                var messages = await client.Me.Chats[chatIdentifier.ChatId].Messages.GetAsync();
                return new GetLastMessages
                {
                    Messages = messages.Value.Take(messagesAmount).Select(m => new ChatMessageDto(m))
                };
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            } 
        }

        [Action("Send message to chat", Description = "Send message to chat")]
        public async Task<ChatMessageDto> SendMessageToChat([ActionParameter] ChatIdentifier chatIdentifier,
            [ActionParameter] SendMessageRequest input)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);
            var requestBody = await CreateChatMessage(client, input);

            try
            {
                var sentMessage = await client.Me.Chats[chatIdentifier.ChatId].Messages.PostAsync(requestBody);
                return new ChatMessageDto(sentMessage);
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            }
        }

        [Action("Delete message from chat", Description = "Delete message from chat")]
        public async Task DeleteMessageFromChat([ActionParameter] ChatIdentifier chatIdentifier, 
            [ActionParameter] MessageIdentifier messageIdentifier)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);

            try
            {
                await client.Me.Chats[chatIdentifier.ChatId].Messages[messageIdentifier.MessageId].SoftDelete.PostAsync();
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            }
        }
        
        private async Task<ChatMessage> CreateChatMessage(MSTeamsClient client, SendMessageRequest input)
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
        
        private async Task<DriveItem> UploadFile(FileReference file)
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
        
            var fileStream = await _fileManagementClient.DownloadAsync(file);
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

            var fileUploadTask =
                new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, chunkSize, client.RequestAdapter);
            var uploadResult = await fileUploadTask.UploadAsync();
            return uploadResult.ItemResponse;
        }
    }
}
