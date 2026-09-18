using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Newtonsoft.Json;

namespace Apps.MicrosoftTeams.DynamicHandlers;

public class ChannelHandler(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, 
        CancellationToken cancellationToken)
    {
        var client = new MSTeamsClient(InvocationContext.AuthenticationCredentialsProviders);
        TeamCollectionResponse? joinedTeams;
        try
        {
            joinedTeams = await client.Me.JoinedTeams.GetAsync(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw MSTeamsClient.ConfigureErrorException(ex);
        }
        var teams = joinedTeams.Value;
        var channelsByTeam = new Dictionary<string, string>?[teams.Count];
        var forbiddenTeams = 0;

        await Parallel.ForEachAsync(Enumerable.Range(0, teams.Count), new ParallelOptions
        {
            MaxDegreeOfParallelism = 3,
            CancellationToken = cancellationToken
        }, async (index, token) =>
        {
            var team = teams[index];
            token.ThrowIfCancellationRequested();
            ChannelCollectionResponse? teamChannels;
            try
            {
                teamChannels = await client.Teams[team.Id].Channels.GetAsync(
                    request => request.QueryParameters.Select = ["id", "displayName"],
                    cancellationToken: token);
            }
            catch (ODataError ex) when (ex.ResponseStatusCode == 403)
            {
                Interlocked.Increment(ref forbiddenTeams);
                InvocationContext.Logger?.LogError(
                    $"[MicrosoftTeamsChannelHandler] Skipping inaccessible team '{team.DisplayName}' ({team.Id}). Graph returned 403: {ex.Error?.Message}",
                    []);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw MSTeamsClient.ConfigureErrorException(ex);
            }

            var teamResult = new Dictionary<string, string>();
            foreach (var channel in teamChannels.Value)
            {
                var key = JsonConvert.SerializeObject(new TeamChannel { TeamId = team.Id, ChannelId = channel.Id });
                teamResult[key] = $"{channel.DisplayName} ({team.DisplayName} team)";
            }
            channelsByTeam[index] = teamResult;
        });

        cancellationToken.ThrowIfCancellationRequested();
        if (forbiddenTeams > 0 && forbiddenTeams == teams.Count)
            throw new PluginApplicationException(
                "You do not have permission to read channels in any of your teams. Please check your Teams access and connection permissions.");

        var channels = new Dictionary<string, string>();
        foreach (var teamResult in channelsByTeam)
        {
            if (teamResult is null)
                continue;

            foreach (var channel in teamResult)
                channels[channel.Key] = channel.Value;
        }

        return channels;
    }
}

public class TeamChannel
{
    public string TeamId { get; set; }
    public string ChannelId { get; set; }
}