using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.MicrosoftTeams;

public class MsTeamsInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds => InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected MSTeamsClient Client { get; }

    protected MsTeamsInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new(Creds);
    }
}