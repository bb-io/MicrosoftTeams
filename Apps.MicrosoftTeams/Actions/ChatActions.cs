using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Responses;
using Azure.Core;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Actions
{
    [ActionList]
    public class ChatActions
    {
        [Action("List chats", Description = "List chats")]
        public async Task<ListChatsResponse> ListChats(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var chats = await client.Me.Chats.GetAsync();
            return new ListChatsResponse()
            {
                Chats = chats.Value.Select(ch => new ChatDto() { Id = ch.Id, Topic = ch.Topic }).ToList()
            };
        }
    }
}
