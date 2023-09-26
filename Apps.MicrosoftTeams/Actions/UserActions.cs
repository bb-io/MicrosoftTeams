using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common;
using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Models.Responses;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.MicrosoftTeams.Actions
{
    [ActionList]
    public class UserActions : BaseInvocable
    {
        private readonly IEnumerable<AuthenticationCredentialsProvider> _authenticationCredentialsProviders;

        protected UserActions(InvocationContext invocationContext) : base(invocationContext)
        {
            _authenticationCredentialsProviders = invocationContext.AuthenticationCredentialsProviders;
        }
        
        [Action("Get my user information", Description = "Get my user information")]
        public async Task<UserDto> GetMe()
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);
            var myInfo = await client.Me.GetAsync();
            return new UserDto(myInfo);
        }

        [Action("List all users", Description = "List all users")]
        public async Task<ListUsersResponse> ListUsers()
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);
            var users = await client.Users.GetAsync();
            return new ListUsersResponse { Users = users.Value.Select(u => new UserDto(u)) };
        }

        [Action("Get user", Description = "Get user by ID")]
        public async Task<UserDto> GetUser([ActionParameter] UserIdentifier userIdentifier)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);
            var user = await client.Users[userIdentifier.UserId].GetAsync();
            return new UserDto(user);
        }

        [Action("Get chat members", Description = "Get chat members")]
        public async Task<GetChatUsersResponse> GetChatUsers([ActionParameter] ChatIdentifier chatIdentifier)
        {
            var client = new MSTeamsClient(_authenticationCredentialsProviders);
            var members = await client.Me.Chats[chatIdentifier.ChatId].Members.GetAsync();
            return new GetChatUsersResponse
            {
                Members = members.Value.Select(m => new ChatMemberDto(m))
            };
        }
    }
}
