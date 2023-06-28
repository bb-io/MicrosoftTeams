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

        [Action("Get chat message", Description = "Get chat message")]
        public async Task<MessageDto> GetChatMessage(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetChatMessageRequest input)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var message = await client.Me.Chats[input.ChatId].Messages[input.MessageId].GetAsync();
            return new MessageDto()
            {
                Id = message?.Id,
                Content = message?.Body?.Content
            };
        }

        [Action("Get last n messages", Description = "Get last n messages")]
        public async Task<GetLastMessages> GetLastMessages(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] GetLastMessagesRequest input)
        {
            var client = new MSTeamsClient(authenticationCredentialsProviders);
            var messages = await client.Me.Chats[input.ChatId].Messages.GetAsync();
            return new GetLastMessages()
            {
                Messages = messages.Value.TakeLast(input.MessagesAmount).Select(m => new MessageDto() { Id = m.Id, Content = m.Body?.Content})
            };
        }

        [Action("Send text message to chat", Description = "Send text message to chat")]
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
