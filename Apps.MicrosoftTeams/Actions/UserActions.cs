using Apps.MicrosoftTeams.Models.Requests;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Responses;

namespace Apps.MicrosoftTeams.Actions
{
    public class UserActions
    {
        [Action("Get my user information", Description = "Get my user information")]
        public async Task<UserDto> GetMe(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var myInfo = await client.Me.GetAsync();
            return new UserDto(myInfo);
        }

        [Action("List all users", Description = "List all users")]
        public async Task<ListUsersResponse> ListUsers(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var users = await client.Users.GetAsync();
            return new ListUsersResponse() {
                Users = users.Value.Select(u => new UserDto(u))
            };
        }

        [Action("Get user", Description = "Get user by ID")]
        public async Task<UserDto> GetUser(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetUserRequest input)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var user = await client.Users[input.UserId].GetAsync();
            return new UserDto(user);
        }

        [Action("Get chat members", Description = "Get chat members")]
        public async Task<GetChatUsersResponse> GetChatUsers(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetChatUsersRequest input)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var members = await client.Me.Chats[input.ChatId].Members.GetAsync();
            return new GetChatUsersResponse()
            {
                Members = members.Value.Select(m => new ChatMemberDto(m))
            };
        }
    }
}
