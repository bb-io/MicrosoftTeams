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
        var requestBody = new ChatMessage
        {
            Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = input.Message
            },
        };

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
}