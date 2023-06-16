using Apps.MicrosoftTeams.Dtos;
using Apps.MicrosoftTeams.Models.Requests;
using Apps.MicrosoftTeams.Models.Responses;
using Azure.Core;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Microsoft.Graph;
using Microsoft.Graph.Models;
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

        [Action("Send message to chat", Description = "Send message to chat")]
        public async Task<SendMessageToChatResponse> SendMessageToChat(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] SendMessageToChatRequest input)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var message = await client.Me.Chats[input.ChatId].Messages.PostAsync(
                new ChatMessage() { Body = new ItemBody() { Content = input.Message } });
            return new SendMessageToChatResponse() { MessageId = message.Id };
        }
    }
}
