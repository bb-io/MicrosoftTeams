using System.Net;
using Polly;
using Polly.Retry;

namespace Apps.MicrosoftTeams;

internal sealed class GraphRetryHandler : DelegatingHandler
{
    // Graph mutations intentionally use at-least-once semantics, so retries are not restricted by HTTP method.
    internal const int MaxRetryAttempts = 3;
    internal static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(300);
    internal static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(2);

    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public GraphRetryHandler() : this(CreateRetryOptions())
    {
    }

    internal GraphRetryHandler(RetryStrategyOptions<HttpResponseMessage> options)
    {
        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(options)
            .Build();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return _pipeline.ExecuteAsync(
            async token => await base.SendAsync(request, token),
            cancellationToken).AsTask();
    }

    internal static RetryStrategyOptions<HttpResponseMessage> CreateRetryOptions()
    {
        return new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = MaxRetryAttempts,
            Delay = BaseDelay,
            MaxDelay = MaxDelay,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => ValueTask.FromResult(IsTransient(args.Outcome, args.Context.CancellationToken)),
            DelayGenerator = args => ValueTask.FromResult(GetRetryAfter(args.Outcome.Result))
        };
    }

    private static bool IsTransient(Outcome<HttpResponseMessage> outcome, CancellationToken cancellationToken)
    {
        if (outcome.Result is { } response)
        {
            return response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
                   || (int)response.StatusCode >= 500;
        }

        return outcome.Exception switch
        {
            HttpRequestException => true,
            IOException => true,
            TimeoutException => true,
            OperationCanceledException when !cancellationToken.IsCancellationRequested => true,
            _ => false
        };
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage? response)
    {
        var retryAfter = response?.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta)
            return ClampDelay(delta);

        if (retryAfter?.Date is not { } date)
            return null;

        var delay = date - DateTimeOffset.UtcNow;
        return ClampDelay(delay);
    }

    private static TimeSpan ClampDelay(TimeSpan delay) =>
        delay <= TimeSpan.Zero ? TimeSpan.Zero : delay < MaxDelay ? delay : MaxDelay;
}
