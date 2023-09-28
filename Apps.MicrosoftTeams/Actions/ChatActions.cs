using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Models.Requests;
using Apps.MicrosoftTeams.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.MicrosoftTeams.Actions
{
    [ActionList]
    public class ChatActions : BaseInvocable
    {
        private readonly IEnumerable<AuthenticationCredentialsProvider> _authenticationCredentialsProviders;

        public ChatActions(InvocationContext invocationContext) : base(invocationContext)
        {
            _authenticationCredentialsProviders = invocationContext.AuthenticationCredentialsProviders;
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

        [Action("Send text message to chat", Description = "Send text message to chat")]
        public async Task<ChatMessageDto> SendMessageToChat([ActionParameter] ChatIdentifier chatIdentifier,
            [ActionParameter] SendMessageRequest input)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);
            var requestBody = new ChatMessage
            {
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = input.Message
                }
            };

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
    }
}
