using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.DynamicHandlers;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Models.Requests;
using Apps.MicrosoftTeams.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.Content;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Newtonsoft.Json;
using Microsoft.Graph.Communications.OnlineMeetings.Item.Recordings;

namespace Apps.MicrosoftTeams.Actions
{
    [ActionList]
    public class CallActions : BaseInvocable
    {
        private readonly IEnumerable<AuthenticationCredentialsProvider> _authenticationCredentialsProviders;
        private readonly IFileManagementClient _fileManagementClient;

        public CallActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
            : base(invocationContext)
        {
            _authenticationCredentialsProviders = invocationContext.AuthenticationCredentialsProviders;
            _fileManagementClient = fileManagementClient;
        }

        [Action("Get meeting recording", Description = "Get meeting recording URL")]
        public async Task<MeetingRecordingDto> GetMeetingRecording([ActionParameter] MeetingIdentifier Meeting
            , [ActionParameter] UserIdentifier User)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);
            
            try
            {
                var response = await client.Users[User.UserId]
                    .OnlineMeetings[Meeting.Id].Recordings.GetAsync();

                return new MeetingRecordingDto(response.Value.First()) ;
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            }
        }

        [Action("Get meeting transcript", Description = "Get meeting transcript")]
        public async Task<FileReference> GetMeetingTranscript([ActionParameter] MeetingIdentifier Meeting
           , [ActionParameter] UserIdentifier User)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);

            try
            {
                var response = await client.Users[User.UserId]
                    .OnlineMeetings[Meeting.Id].Transcripts.GetAsync();
                var content = await client.Users[User.UserId]
                    .OnlineMeetings[Meeting.Id].Transcripts[response.Value.First().Id].Content.GetAsync();
                var file = await  _fileManagementClient.UploadAsync(content, "text/vtt", "transcript.vtt");
                    return file;
            }
            catch (ODataError error)
            {
                throw new Exception(error.Error.Message);
            }
        }

        
    }
}
