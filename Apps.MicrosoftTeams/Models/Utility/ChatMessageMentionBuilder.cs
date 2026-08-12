using Apps.MicrosoftTeams.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Microsoft.Graph.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace Apps.MicrosoftTeams.Models.Utility;

public static partial class ChatMessageMentionBuilder
{
    [GeneratedRegex(@"<at\s+(?:user_id\s*=\s*""([^""]+)""|id\s*=\s*""(\d+)"")\s*>([^<]*)</at>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex InlineMentionRegex();

    public static async Task ApplyAsync(
        MSTeamsClient client,
        ChatMessage chatMessage,
        SendMessageRequest input,
        CancellationToken cancellationToken = default)
    {
        var appendedUsers = new List<User>();
        var mentionedUserIds = input.MentionedUserIds?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        foreach (var userId in mentionedUserIds)
        {
            var user = await client.ExecuteWithErrorHandlingAsync(() =>
                client.Users[userId].GetAsync(cancellationToken: cancellationToken));

            if (user is null || string.IsNullOrWhiteSpace(user.Id) || string.IsNullOrWhiteSpace(user.DisplayName))
                throw new PluginApplicationException($"Could not resolve mentioned user '{userId}'.");

            appendedUsers.Add(user);
        }

        Apply(chatMessage, input.Message, appendedUsers);
    }

    public static void Apply(ChatMessage chatMessage, string? message, IEnumerable<User>? appendedUsers = null)
    {
        var mentions = new List<ChatMessageMention>();
        var inlineUserIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var availableAppendedUsers = appendedUsers?
            .Where(user => !string.IsNullOrWhiteSpace(user.Id))
            .GroupBy(user => user.Id!, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray() ?? [];

        var content = InlineMentionRegex().Replace(message ?? string.Empty, match =>
        {
            string userId;
            string displayName;

            if (match.Groups[1].Success)
            {
                userId = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
                displayName = WebUtility.HtmlDecode(match.Groups[3].Value).Trim();
            }
            else
            {
                var selectedUserIndex = int.Parse(match.Groups[2].Value);
                if (selectedUserIndex >= availableAppendedUsers.Length)
                    throw new PluginMisconfigurationException(
                        $"The inline mention with id '{selectedUserIndex}' does not have a matching user in Mentioned users.");

                var selectedUser = availableAppendedUsers[selectedUserIndex];
                userId = selectedUser.Id!;
                displayName = selectedUser.DisplayName;
            }

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(displayName))
                throw new PluginMisconfigurationException(
                    "Inline user mentions must contain both a user ID and a display name.");

            var mentionId = mentions.Count;
            mentions.Add(CreateMention(mentionId, userId, displayName));
            inlineUserIds.Add(userId);

            return $"<at id=\"{mentionId}\">{WebUtility.HtmlEncode(displayName)}</at>";
        });

        var usersToAppend = availableAppendedUsers
            .Where(user => !inlineUserIds.Contains(user.Id!))
            .ToArray();

        foreach (var user in usersToAppend)
        {
            if (string.IsNullOrWhiteSpace(user.DisplayName))
                throw new PluginApplicationException(
                    $"Could not resolve the display name for mentioned user '{user.Id}'.");

            if (content.Length > 0 && !char.IsWhiteSpace(content[^1]))
                content += " ";

            var mentionId = mentions.Count;
            content += $"<at id=\"{mentionId}\">{WebUtility.HtmlEncode(user.DisplayName)}</at>";
            mentions.Add(CreateMention(mentionId, user.Id!, user.DisplayName));
        }

        chatMessage.Body ??= new ItemBody { ContentType = BodyType.Html };
        chatMessage.Body.ContentType = BodyType.Html;
        chatMessage.Body.Content = content;
        chatMessage.Mentions = mentions.Count > 0 ? mentions : null;
    }

    private static ChatMessageMention CreateMention(int mentionId, string userId, string displayName)
    {
        return new ChatMessageMention
        {
            Id = mentionId,
            MentionText = displayName,
            Mentioned = new ChatMessageMentionedIdentitySet
            {
                User = new Identity
                {
                    Id = userId,
                    DisplayName = displayName,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "userIdentityType", "aadUser" }
                    }
                }
            }
        };
    }
}
