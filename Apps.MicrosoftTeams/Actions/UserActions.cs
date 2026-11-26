using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common;
using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Identifiers;
using Apps.MicrosoftTeams.Models.Responses;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.MicrosoftTeams.Actions;

[ActionList("Users")]
public class UserActions(InvocationContext invocationContext) : MsTeamsInvocable(invocationContext)
{
    [Action("Get my user information", Description = "Get my user information")]
    public async Task<UserDto> GetMe()
    {
        var myInfo = await Client.ExecuteWithErrorHandlingAsync(() => Client.Me.GetAsync());
        return new UserDto(myInfo);
    }

    [Action("List all users", Description = "List all users")]
    public async Task<ListUsersResponse> ListUsers()
    {
        var users = await Client.ExecuteWithErrorHandlingAsync(() => Client.Users.GetAsync());
        return new ListUsersResponse { Users = users.Value.Select(u => new UserDto(u)) };
    }

    [Action("Get user", Description = "Get user by ID")]
    public async Task<UserDto> GetUser([ActionParameter] UserIdentifier userIdentifier)
    {
        var user = await Client.ExecuteWithErrorHandlingAsync(() => Client.Users[userIdentifier.UserId].GetAsync());
        return new UserDto(user);
    }

    [Action("Find user by email", Description = "Find a user by email address")]
    public async Task<UserDto> FindUserByEmail([ActionParameter] UserEmailIdentifier userEmailIdentifier)
    {
        var email = userEmailIdentifier.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            throw new PluginMisconfigurationException("Email must not be empty.");

        var escapedEmail = email.Replace("'", "''");
        var users = await Client.ExecuteWithErrorHandlingAsync(() => Client.Users.GetAsync(requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Filter =
                $"mail eq '{escapedEmail}' or userPrincipalName eq '{escapedEmail}'";
            requestConfiguration.QueryParameters.Top = 2;
        }));

        var matches = users?.Value ?? [];
        if (matches.Count == 0)
            throw new PluginApplicationException($"No user was found with the email address '{email}'.");

        if (matches.Count > 1)
            throw new PluginApplicationException($"Multiple users were found with the email address '{email}'.");

        return new UserDto(matches[0]);
    }

    [Action("Get chat members", Description = "Get chat members")]
    public async Task<GetChatUsersResponse> GetChatUsers([ActionParameter] ChatIdentifier chatIdentifier)
    {
        var members = await Client.ExecuteWithErrorHandlingAsync(() => Client.Me.Chats[chatIdentifier.ChatId].Members.GetAsync());
        return new GetChatUsersResponse
        {
            Members = members.Value.Select(m => new ChatMemberDto(m))
        };
    }

    [Action("Debug", Description = "Debug")]
    public List<AuthenticationCredentialsProvider> GetAuthenticationCredentialsProviders(){return InvocationContext.AuthenticationCredentialsProviders.ToList();}
}
