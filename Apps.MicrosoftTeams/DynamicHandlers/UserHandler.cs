using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.MicrosoftTeams.DynamicHandlers
{
    public class UserHandler : BaseInvocable, IAsyncDataSourceHandler
    {
        public UserHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, 
            CancellationToken cancellationToken)
        {
            var client = new MSTeamsClient(InvocationContext.AuthenticationCredentialsProviders);
            var users = await client.Users.GetAsync(requestConfiguration =>
            {
                if (!string.IsNullOrWhiteSpace(context.SearchString))
                {
                    requestConfiguration.QueryParameters.Search = $"\"displayName:{context.SearchString}\"";
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                }
            }, cancellationToken);
            return users.Value.ToDictionary(k => k.Id, v => v.DisplayName);
        }
    }
}
